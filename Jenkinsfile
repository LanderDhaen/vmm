pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO = '1'
        ANSIBLE_HOST_KEY_CHECKING = 'False'
        SSH_KEY = '/etc/jenkins_jobs/jenkins_rsa'
        INFRA_REPO = 'git@github.com:HOGENT-RISE/ops-2526-vc1-1.git'
        APP_REPO = 'git@github.com:HOGENT-RISE/dotnet-2526-vc2.git'
        NUGET_PACKAGES = "${WORKSPACE}/.nuget/packages"
        DOTNET_CLI_HOME = "${WORKSPACE}/dotnet_home"
    }

    options {
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
    }

    stages {
        stage('Initialize') {
            steps {
                script {
                    currentBuild.displayName = "#${BUILD_NUMBER} - ${env.GIT_BRANCH ?: 'main'}"
                }
                sh '''
                    echo "========================================"
                    echo "   RISE .NET CI/CD PIPELINE"
                    echo "   Build #${BUILD_NUMBER}"
                    echo "========================================"
                    mkdir -p "${NUGET_PACKAGES}" "${DOTNET_CLI_HOME}"
                '''
            }
        }

        stage('Checkout') {
            parallel {
                stage('Application Code') {
                    steps {
                        dir('app') {
                            sh '''
                                export GIT_SSH_COMMAND="ssh -i ${SSH_KEY} -o StrictHostKeyChecking=no"
                                git clone --depth=1 --single-branch ${APP_REPO} .
                            '''
                            script {
                                env.COMMIT_SHA = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()
                                env.COMMIT_SHORT = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                                env.COMMIT_MSG = sh(script: 'git log -1 --pretty=%B', returnStdout: true).trim()
                                env.COMMIT_AUTHOR = sh(script: 'git log -1 --pretty=%an', returnStdout: true).trim()
                            }
                            echo "Commit: ${env.COMMIT_SHA}"
                            echo "Author: ${env.COMMIT_AUTHOR}"
                            echo "Message: ${env.COMMIT_MSG}"
                        }
                    }
                }
                stage('Infrastructure Code') {
                    steps {
                        dir('infrastructure') {
                            sh '''
                                export GIT_SSH_COMMAND="ssh -i ${SSH_KEY} -o StrictHostKeyChecking=no"
                                git clone --depth=1 ${INFRA_REPO} .
                            '''
                        }
                    }
                }
            }
        }

        stage('Environment Detection') {
            steps {
                script {
                    def ansibleDir = "${WORKSPACE}/infrastructure/src/ansible"
                    if (params.DEPLOY_ENV == 'dev') {
                        env.INVENTORY_FILE = 'inventory_dev.yml'
                        env.TARGET_ENV = 'dev'
                    } else {
                        // auto, prod, or any other value defaults to prod
                        env.INVENTORY_FILE = 'inventory_prod.yml'
                        env.TARGET_ENV = 'prod'
                    }
                    env.ANSIBLE_DIR = ansibleDir
                    dir(ansibleDir) {
                        // Get inventory info for SSH connections
                        env.DB_INTERNAL_IP = sh(script: "grep 'real_db_server_ip:' ${env.INVENTORY_FILE} | head -1 | cut -d ':' -f 2 | tr -d ' \"'", returnStdout: true).trim()
                        def dbInfo = sh(script: "ansible-inventory -i ${env.INVENTORY_FILE} --host database", returnStdout: true).trim()
                        env.DB_SSH_HOST = sh(script: "echo '${dbInfo}' | python3 -c \"import sys, json; d=json.load(sys.stdin); print(d.get('ansible_host',''))\"", returnStdout: true).trim()
                        env.DB_SSH_USER = sh(script: "echo '${dbInfo}' | python3 -c \"import sys, json; d=json.load(sys.stdin); print(d.get('ansible_user','vicuser'))\"", returnStdout: true).trim()
                        env.DB_SSH_PORT = sh(script: "echo '${dbInfo}' | python3 -c \"import sys, json; d=json.load(sys.stdin); print(d.get('ansible_port','22'))\"", returnStdout: true).trim()
                        def appInfo = sh(script: "ansible-inventory -i ${env.INVENTORY_FILE} --host application", returnStdout: true).trim()
                        env.APP_SSH_HOST = sh(script: "echo '${appInfo}' | python3 -c \"import sys, json; d=json.load(sys.stdin); print(d.get('ansible_host',''))\"", returnStdout: true).trim()
                        env.APP_SSH_USER = sh(script: "echo '${appInfo}' | python3 -c \"import sys, json; d=json.load(sys.stdin); print(d.get('ansible_user','vicuser'))\"", returnStdout: true).trim()
                        env.APP_SSH_PORT = sh(script: "echo '${appInfo}' | python3 -c \"import sys, json; d=json.load(sys.stdin); print(d.get('ansible_port','22'))\"", returnStdout: true).trim()
                    }
                    echo "Target Environment: ${env.TARGET_ENV}"
                    echo "Inventory: ${env.INVENTORY_FILE}"
                    echo "Database: ${env.DB_SSH_USER}@${env.DB_SSH_HOST}:${env.DB_SSH_PORT}"
                    echo "Application: ${env.APP_SSH_USER}@${env.APP_SSH_HOST}:${env.APP_SSH_PORT}"
                }
            }
        }

        stage('Fetch Secrets from OpenBao') {
            steps {
                script {
                    // Fetch OpenBao token from db server
                    def vaultToken = sh(script: """
                        ssh -p ${DB_SSH_PORT} -i ${SSH_KEY} -o StrictHostKeyChecking=no \
                            ${DB_SSH_USER}@${DB_SSH_HOST} \
                            "sudo cat /root/openbao_init.json" | python3 -c "import sys, json; print(json.load(sys.stdin)['root_token'])"
                    """, returnStdout: true).trim()

                    // Fetch MSSQL SA password from OpenBao
                    env.DB_PASSWORD = sh(script: """
                        ssh -p ${DB_SSH_PORT} -i ${SSH_KEY} -o StrictHostKeyChecking=no \
                            ${DB_SSH_USER}@${DB_SSH_HOST} \
                            "curl -s -H 'X-Vault-Token: ${vaultToken}' http://localhost:8200/v1/secret/data/db/mssql" \
                            | python3 -c "import sys, json; print(json.load(sys.stdin)['data']['data']['sa_password'])"
                    """, returnStdout: true).trim()

                    // Fetch RiseDB credentials from OpenBao
                    env.RISEDB_PASSWORD = sh(script: """
                        ssh -p ${DB_SSH_PORT} -i ${SSH_KEY} -o StrictHostKeyChecking=no \
                            ${DB_SSH_USER}@${DB_SSH_HOST} \
                            "curl -s -H 'X-Vault-Token: ${vaultToken}' http://localhost:8200/v1/secret/data/db/risedb" \
                            | python3 -c "import sys, json; print(json.load(sys.stdin)['data']['data']['password'])"
                    """, returnStdout: true).trim()

                    echo ">>> Secrets fetched from OpenBao successfully"
                }
            }
        }

        stage('Restore Dependencies') {
            steps {
                dir('app') {
                    sh '''
                        echo ">>> Restoring NuGet packages..."
                        dotnet restore ./src/Rise.Server/Rise.Server.csproj --packages "${NUGET_PACKAGES}"
                    '''
                }
            }
        }

        stage('Build') {
            steps {
                dir('app') {
                    sh '''
                        echo ">>> Building application..."
                        dotnet build ./src/Rise.Server/Rise.Server.csproj \
                            -c Release \
                            --no-restore \
                            /p:TreatWarningsAsErrors=false \
                            /p:WarningLevel=0
                    '''
                }
            }
            post {
                success {
                    echo "Build completed successfully"
                }
                failure {
                    error "Build failed - stopping pipeline"
                }
            }
        }

        stage('Database Migration') {
            steps {
                dir('app') {
                    sh """
                        echo ">>> Preparing database migration tools..."
                        dotnet new tool-manifest --force
                        dotnet tool install dotnet-ef --version 9.0.10
                        export PATH="\$PATH:\$HOME/.dotnet/tools"
                        echo ">>> Dropping existing database (clean slate)..."
                        ssh -p \${DB_SSH_PORT} -i \${SSH_KEY} -o StrictHostKeyChecking=no \\
                            \${DB_SSH_USER}@\${DB_SSH_HOST} \\
                            'sudo docker exec -i mssql_server /opt/mssql-tools18/bin/sqlcmd \\
                                -S localhost -U sa -P \"'\${DB_PASSWORD}'\" -C \\
                                -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = N'"'"'RiseDB'"'"') \\
                                    BEGIN ALTER DATABASE [RiseDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; \\
                                    DROP DATABASE [RiseDB]; END"'
                        echo ">>> Applying database migrations..."
                        export ConnectionStrings__DatabaseConnection="Server=\${DB_INTERNAL_IP},1433;Database=RiseDB;User Id=sa;Password=\${DB_PASSWORD};TrustServerCertificate=true;Encrypt=False;"
                        ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \\
                            --project src/Rise.Persistence \\
                            --startup-project src/Rise.Server \\
                            --configuration Release
                        echo ">>> Seeding database..."
                        timeout 30s dotnet run \\
                            --project src/Rise.Server \\
                            --configuration Release \\
                            --no-build \\
                            --environment Development || true
                        echo ">>> Granting riseuser access to RiseDB..."
                        ssh -p \${DB_SSH_PORT} -i \${SSH_KEY} -o StrictHostKeyChecking=no \\
                            \${DB_SSH_USER}@\${DB_SSH_HOST} \\
                            'sudo docker exec -i mssql_server /opt/mssql-tools18/bin/sqlcmd \\
                                -S localhost -U sa -P \"'\${DB_PASSWORD}'\" -C -d RiseDB \\
                                -Q "IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '"'"'riseuser'"'"') \\
                                    CREATE USER riseuser FOR LOGIN riseuser; \\
                                    EXEC sp_addrolemember '"'"'db_owner'"'"', '"'"'riseuser'"'"';"'
                    """
                }
            }
        }

        stage('Publish Artifacts') {
            steps {
                dir('app') {
                    sh '''
                        echo ">>> Configuring for production..."
                        sed -i "s|https://localhost:5001|https://vc1.vichogent.be/|g" \
                            ./src/Rise.Client/wwwroot/appsettings.json
                        echo ">>> Publishing application..."
                        dotnet publish ./src/Rise.Server/Rise.Server.csproj \
                            -c Release \
                            -o ./publish \
                            /p:UseAppHost=false \
                            /p:PublishTrimmed=false
                        echo ">>> Creating deployment artifact..."
                        tar -czf "${WORKSPACE}/publish.tar.gz" -C publish .
                    '''
                    archiveArtifacts artifacts: 'publish/**/*', fingerprint: true
                }
                sh '''
                    echo ">>> Artifact created: publish.tar.gz"
                    ls -lh "${WORKSPACE}/publish.tar.gz"
                '''
            }
        }

        stage('Deploy') {
            stages {
                stage('Pre-flight Check') {
                    steps {
                        sh '''
                            echo ">>> Pre-flight connectivity check..."
                            ssh -p ${APP_SSH_PORT} -i ${SSH_KEY} -o StrictHostKeyChecking=no \
                                ${APP_SSH_USER}@${APP_SSH_HOST} \
                                "bash -c 'echo Application_server_reachable && sudo k3s kubectl get nodes --request-timeout=5s'" \
                                || { echo "ERROR: Cannot reach application server or K8s cluster"; exit 1; }
                        '''
                    }
                }
                stage('Blue/Green Deployment') {
                    steps {
                        dir("${env.ANSIBLE_DIR}") {
                            sh '''
                                echo ">>> Executing Blue/Green deployment via Ansible..."
                                mkdir -p ~/.ssh
                                cp "${SSH_KEY}" ~/.ssh/id_ed25519
                                chmod 600 ~/.ssh/id_ed25519
                                ansible-playbook k8s-deploy-blue-green.yml \
                                    -i "${INVENTORY_FILE}" \
                                    --private-key "${SSH_KEY}" \
                                    -e "artifact_archive=${WORKSPACE}/publish.tar.gz" \
                                    -e "commit_sha=${COMMIT_SHORT}" \
                                    -e "build_number=${BUILD_NUMBER}"
                            '''
                        }
                    }
                }
                stage('Health Check') {
                    steps {
                        script {
                            def maxRetries = 10
                            def retryInterval = 15
                            for (int i = 1; i <= maxRetries; i++) {
                                try {
                                    sh """
                                        echo ">>> Health check attempt ${i}/${maxRetries}..."
                                        HEALTH_STATUS=\$(ssh -p ${APP_SSH_PORT} -i ${SSH_KEY} -o StrictHostKeyChecking=no \
                                            ${APP_SSH_USER}@${APP_SSH_HOST} \
                                            "sudo k3s kubectl get pods -n rise-app -l app=rise-app -o jsonpath='{.items[*].status.phase}' 2>/dev/null || echo 'Unknown'")
                                        echo "Pod status: \${HEALTH_STATUS}"
                                        if echo "\${HEALTH_STATUS}" | grep -q "Running"; then
                                            echo "Health check PASSED"
                                            exit 0
                                        else
                                            echo "Pods not ready yet..."
                                            exit 1
                                        fi
                                    """
                                    break
                                } catch (Exception e) {
                                    if (i == maxRetries) {
                                        error "Health check failed after ${maxRetries} attempts"
                                    }
                                    echo "Retrying in ${retryInterval} seconds..."
                                    sleep(retryInterval)
                                }
                            }
                        }
                    }
                }
            }
        }

        stage('Verification') {
            steps {
                sh '''
                    echo "========================================"
                    echo "   DEPLOYMENT VERIFICATION"
                    echo "========================================"
                    SSH_CMD="ssh -i ${SSH_KEY} -o StrictHostKeyChecking=no -p ${APP_SSH_PORT} ${APP_SSH_USER}@${APP_SSH_HOST}"
                    ACTIVE_ENV=$($SSH_CMD "cat /etc/rise-app/active_env 2>/dev/null || echo 'unknown'")
                    echo "Active Environment: ${ACTIVE_ENV}"
                    echo ""
                    echo ">>> K8s Pods:"
                    $SSH_CMD "sudo k3s kubectl get pods -n rise-app -o wide" || true
                    echo ""
                    echo ">>> K8s Deployments:"
                    $SSH_CMD "sudo k3s kubectl get deployments -n rise-app" || true
                    echo ""
                    echo ">>> K8s Services:"
                    $SSH_CMD "sudo k3s kubectl get services -n rise-app" || true
                    echo ""
                    echo ">>> Gateway Status:"
                    if $SSH_CMD "sudo docker ps | grep caddy-gateway > /dev/null"; then
                        echo "Gateway (caddy-gateway) is RUNNING"
                    else
                        echo "WARNING: Gateway (caddy-gateway) is NOT running!"
                        $SSH_CMD "sudo docker logs caddy-gateway --tail 20" || true
                    fi
                '''
            }
        }
    }

    post {
        success {
            script {
                currentBuild.description = "Deployed ${env.COMMIT_SHORT} to ${env.TARGET_ENV}"
            }
            echo """
                ========================================
                   DEPLOYMENT COMPLETED SUCCESSFULLY
                   Environment: ${env.TARGET_ENV}
                   Commit: ${env.COMMIT_SHORT}
                   URL: https://vc1.vichogent.be
                ========================================
            """
            sh """
                curl -s -H 'Content-Type: application/json' -X POST \
                    -d '{"embeds":[{"title":"Rise .NET Deployment - SUCCESS","description":"Deployed ${COMMIT_SHORT} to ${TARGET_ENV}","color":3066993,"fields":[{"name":"Build","value":"#${BUILD_NUMBER}","inline":true},{"name":"Environment","value":"${TARGET_ENV}","inline":true},{"name":"Commit","value":"${COMMIT_SHORT}","inline":true}],"footer":{"text":"Jenkins CI/CD"}}]}' \
                    'https://discord.com/api/webhooks/1433444252938928229/2Y9ZS89WmgVpB9R4IQkekwunm3nKG_P6ADWKE8q6qpchE35KLg37ynWerIMz5DnNzfTV' || true
            """
        }
        failure {
            script {
                currentBuild.description = "FAILED: ${env.COMMIT_SHORT}"
            }
            echo """
                ========================================
                   DEPLOYMENT FAILED
                   Check logs for details
                ========================================
            """
            sh '''
                curl -s -H 'Content-Type: application/json' -X POST \
                    -d '{"embeds":[{"title":"Rise .NET Deployment - FAILED","description":"Build failed","color":15158332,"fields":[{"name":"Build","value":"#'"${BUILD_NUMBER}"'","inline":true},{"name":"Environment","value":"'"${TARGET_ENV:-unknown}"'","inline":true}],"footer":{"text":"Jenkins CI/CD"}}]}' \
                    'https://discord.com/api/webhooks/1433444252938928229/2Y9ZS89WmgVpB9R4IQkekwunm3nKG_P6ADWKE8q6qpchE35KLg37ynWerIMz5DnNzfTV' || true
            '''
        }
        always {
            script {
                if (currentBuild.result != 'FAILURE') {
                    deleteDir()
                }
            }
        }
    }
}


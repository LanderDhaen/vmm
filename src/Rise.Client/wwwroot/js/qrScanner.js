let qrScanner = null;

window.qrScanner = {
	start: async function (elementId, dotNetRef) {
		if (!window.Html5Qrcode) {
			console.error("html5-qrcode not loaded");
			return;
		}

		qrScanner = new Html5Qrcode(elementId);

		const config = {
			fps: 10,
			qrbox: { width: 250, height: 250 },
			rememberLastUsedCamera: true,
			
			formatsToSupport: [
				Html5QrcodeSupportedFormats.QR_CODE,
				Html5QrcodeSupportedFormats.CODE_128,
				Html5QrcodeSupportedFormats.CODE_39,
				Html5QrcodeSupportedFormats.EAN_13,
				Html5QrcodeSupportedFormats.EAN_8,
				Html5QrcodeSupportedFormats.UPC_A,
				Html5QrcodeSupportedFormats.UPC_E
			]
		};

		try {
			await qrScanner.start(
				{ facingMode: "environment" }, 
				config,
				(decodedText) => {
					dotNetRef.invokeMethodAsync("OnQrScanned", decodedText);
					window.qrScanner.stop();
				}
			);
		} catch (err) {
			console.error("QR start error", err);
			dotNetRef.invokeMethodAsync("OnQrError", err.toString());
		}
	},

	stop: async function () {
		try {
			if (qrScanner) {
				await qrScanner.stop();
				await qrScanner.clear();
				qrScanner = null;
			}
		} catch (err) {
			console.warn("QR stop error", err);
		}
	}
};

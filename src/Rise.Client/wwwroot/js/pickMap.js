window.pickMap = {
	init: function (divId, lat, lng, dotnetRef) {
		const map = L.map(divId).setView([lat, lng], 12);

		L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
			attribution: "&copy; OpenStreetMap contributors"
		}).addTo(map);

		let marker = L.marker([lat, lng]).addTo(map);

		map.on("click", function (e) {
			const { lat, lng } = e.latlng;

			marker.setLatLng([lat, lng]);

			dotnetRef.invokeMethodAsync("OnMapClicked", lat, lng);
		});
	}
};

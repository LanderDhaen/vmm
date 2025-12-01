window.pickMap = {
	init: function (divId, lat, lng, dotnetRef) {
		const map = L.map(divId).setView([lat, lng], 12);

		L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
			attribution: "&copy; OpenStreetMap contributors"
		}).addTo(map);

		let marker = L.marker([lat, lng]).addTo(map);

		L.Control.geocoder({
			defaultMarkGeocode: false,
			placeholder: "Zoek een plaats of adres...",
			collapsed: false
		})
			.on("markgeocode", function (e) {
				const center = e.geocode.center;
				const newLat = center.lat;
				const newLng = center.lng;

				map.setView([newLat, newLng], 16);
				marker.setLatLng([newLat, newLng]);

				dotnetRef.invokeMethodAsync("OnMapClicked", newLat, newLng);
			})
			.addTo(map);

		map.on("click", function (e) {
			const {lat, lng} = e.latlng;
			marker.setLatLng([lat, lng]);
			dotnetRef.invokeMethodAsync("OnMapClicked", lat, lng);
		});
	}
};

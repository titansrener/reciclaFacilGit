
function initialize(cooperativas) {
    var markers = [];
    var mapa = document.getElementById("mapa");
    mapa.style.height = "420px";

    
    
    directionsDisplay = new google.maps.DirectionsRenderer();

    latlng = new google.maps.LatLng(-10.9483281, -37.086572); // coordenada de Aracaju

    var options = {
        zoom: 12,
        center: latlng,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    map = new google.maps.Map(document.getElementById("mapa"), options);

    for (var i = 0; i < cooperativas.length; i++) {
        markers[i] = new google.maps.Marker({
            position: new google.maps.LatLng(cooperativas[i].latitude, cooperativas[i].longitude),
            url: cooperativas[i].url,
            title: cooperativas[i].nome,
            map: map
        });
    }

    for (i = 0; i < markers.length; i++) {
        google.maps.event.addListener(markers[i], 'click', function () {
            window.location.href = this.url;  
        });
    }

    //posicaoAtual();

};

function posicaoAtual() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {

            latlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude); // local atual

            map.setCenter(latlng);

            var geocoder = new google.maps.Geocoder();

            geocoder.geocode({
                "location": new google.maps.LatLng(position.coords.latitude, position.coords.longitude)
            },
            function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    $("#txtEnderecoPartida").val(results[0].formatted_address);
                }
            });

        });
    }
};
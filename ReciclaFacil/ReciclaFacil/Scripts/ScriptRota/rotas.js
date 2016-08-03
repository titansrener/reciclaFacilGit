var map;
var latlng
var directionsDisplay;
var directionsService = new google.maps.DirectionsService();

function initialize(cooperativaCoord, clientesCoord) {	
    directionsDisplay = new google.maps.DirectionsRenderer();

    latlng = new google.maps.LatLng(cooperativaCoord.latitude, cooperativaCoord.longitude); // coordenada da cooperativa
        
    var options = {
        zoom: 15,
        center: latlng,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    
    map = new google.maps.Map(document.getElementById("mapa"), options);

    posicaoAtual(cooperativaCoord, clientesCoord);
    calcularRota(latlng, clientesCoord); // inicio = coordenada da cooperativa

}

function posicaoAtual(cooperativaCoord, clientesCoord) {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {

            latlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude); // local atual

            map.setCenter(latlng);

            calcularRota(latlng, clientesCoord); // inicio = coordenada atual;

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

function calcularRota(inicio, clientesCoord) {
    var enderecoPartida = inicio;

    var waypts = [];

    //WAYPOINTS caso haja mais de um cliente

    if (clientesCoord.length > 1) {
        for (var i = 0; i < clientesCoord.length - 1; i++) {
            waypts.push({
                location: new google.maps.LatLng(clientesCoord[i].latitude, clientesCoord[i].longitude),
                stopover: true
            });
        }
    }

    // chegada = ultimo cliente
    var enderecoChegada = new google.maps.LatLng(clientesCoord[clientesCoord.length - 1].latitude, clientesCoord[clientesCoord.length - 1].longitude)

    var request = {
        origin: enderecoPartida,
        destination: enderecoChegada,
        waypoints: waypts,
        optimizeWaypoints: true,
        provideRouteAlternatives: true,
        travelMode: google.maps.TravelMode.DRIVING
    };

    directionsService.route(request, function (result, status) {
        if (status == google.maps.DirectionsStatus.OK) {
            directionsDisplay.setDirections(result);
        }
    });

    directionsDisplay.setMap(map);
    directionsDisplay.setPanel(document.getElementById("trajeto-texto"));
};

//initialize();
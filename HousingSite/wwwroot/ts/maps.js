/// <reference path="types/MicrosoftMaps/Microsoft.Maps.All.d.ts" />
var dotNetHelper;
var bingMap;
var sessionKey;
var userpinCoords;
var mapClicked;
var addLocOnClick;
var userLayer;
var userPin;
var userInfoBox;
var centerLocation;
var circleCenterRect;
var circlePolygon;
var houseLayer;
var generalLayer;
var isochroneLayer;
var specificLayer;
var specificEnabled;
var chosenHouseLocation;
var chosenHouseRect;
var directionsManager;
var directionsManager1;
var directionsManager2;
var directionsManager3;
var infoboxArray;
var infoboxTemplate = '<div class="customInfoBox" id="infoboxText">' +
    '<b id="infoboxTitle" style="position: absolute; top: 10px; left: 10px; width: 220px;">{title}</b>' +
    '<a id="infoboxDescription" style="position: absolute; top: 30px; left: 10px; width: 220px;">{description}</a></div>';
var isochronePolygon;
var houses;
//let housePins: Microsoft.Maps.Pushpin[];
var housePinData;
//function CreatePushpinData(config: IPushpinData): { pushpin: Microsoft.Maps.Pushpin; infobox: Microsoft.Maps.Infobox } {
//    let pushpin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(0, 0), { color: "Green", visible: false });
//    let infobox = new Microsoft.Maps.Infobox(new Microsoft.Maps.Location(0, 0), {visible: false});
//    let newData = { pushpin, infobox };
//    if (config.pushpin) { newData.pushpin = config.pushpin; }
//    if (config.infobox) { newData.infobox = config.infobox; }
//    return newData;
//}
//#region Testing
//function InsertPinFunction(index: number): void {
//    let pin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(0, 0), { color: "Green" });
//    InsertHousePin(index, pin);
//}
function SetDotNetHelper(ref) {
    dotNetHelper = ref;
}
function FromNet(option) {
    console.log("Received " + option);
    if (dotNetHelper != null) {
        dotNetHelper.invokeMethodAsync("FromJS", "Hi");
    }
    else
        (console.log("No dotnet helper!"));
    //funcRef.invokeMethodAsync("FromJS", "Hi");
}
function FromNet2(funcRef, option) {
    console.log("Received " + option);
    funcRef.invokeMethodAsync("FromJS", "Hi 2");
}
//#endregion
//#region Base class and LoadMap
var BingMap = /** @class */ (function () {
    function BingMap(key) {
        this.map = new Microsoft.Maps.Map('#myMap', {
            credentials: key,
            center: new Microsoft.Maps.Location(52.215951, 6.004184),
            mapTypeId: Microsoft.Maps.MapTypeId.road,
            zoom: 9,
        });
        if (!sessionKey) {
            this.map.getCredentials(function (credentials) {
                sessionKey = credentials;
            });
        }
    }
    return BingMap;
}());
function loadMapScenario(key) {
    bingMap = new BingMap(key);
    isochroneLayer = new Microsoft.Maps.Layer();
    bingMap.map.layers.insert(isochroneLayer);
    isochronePolygon = [];
    generalLayer = new Microsoft.Maps.Layer();
    bingMap.map.layers.insert(generalLayer);
    specificLayer = new Microsoft.Maps.Layer();
    bingMap.map.layers.insert(specificLayer);
    specificEnabled = false;
    userLayer = new Microsoft.Maps.Layer();
    bingMap.map.layers.insert(userLayer);
    userPin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(0, 0), { color: "Green" });
    userInfoBox = new Microsoft.Maps.Infobox(new Microsoft.Maps.Location(0, 0));
    userInfoBox.setMap(bingMap.map);
    houseLayer = new Microsoft.Maps.Layer();
    bingMap.map.layers.insert(houseLayer);
    infoboxArray = [];
    Microsoft.Maps.Events.addHandler(userPin, 'click', function () { userInfoBox.setOptions({ visible: true }); });
    Microsoft.Maps.Events.addHandler(bingMap.map, 'click', function (e) {
        console.log("addLocOnClick: " + addLocOnClick);
        if (e.targetType == "map" && addLocOnClick) {
            mapClicked = "Yes";
            var loc = e.location;
            userLayer.clear();
            AddUserLocation(loc);
            DrawUserLocationCircle(loc);
            userpinCoords = [];
            userpinCoords.push(loc.latitude.toString());
            userpinCoords.push(loc.longitude.toString());
        }
        else
            mapClicked = "No";
    });
    addLocOnClick = false;
}
//#endregion
function GetSessionKey() { return sessionKey; }
function GetUserpinCoords() { return userpinCoords; }
function GetMapClicked() {
    var curMapClicked = mapClicked;
    mapClicked = "No data";
    return curMapClicked;
}
function AddUserLocationOnClick(enable) {
    addLocOnClick = enable;
}
//#region Clear functions
function ClearMap() {
    bingMap.map.entities.clear();
    generalLayer.clear();
    isochroneLayer.clear();
    ClearUserInfo();
}
function ClearHouses() {
    houseLayer.clear();
    if (infoboxArray != null) {
        for (var _i = 0, infoboxArray_1 = infoboxArray; _i < infoboxArray_1.length; _i++) {
            var infoBox = infoboxArray_1[_i];
            infoBox.setOptions({ visible: false });
        }
        infoboxArray.length = 0;
    }
}
function ClearUserInfo() {
    userLayer.clear();
    userInfoBox.setOptions({ visible: false });
    userPin.setOptions({ visible: false });
}
//#endregion
function RedrawCircle(place, radius) {
    GotoAddress(place);
}
function GotoAddress(address) {
    var l = Microsoft.Maps.LocationRect.fromLocations(new Microsoft.Maps.Location(address[0], address[1]));
    bingMap.map.setView({ bounds: l });
}
//#region User location
function AddUserLocation(userloc, text) {
    if (text === void 0) { text = "Clicked location (loading address..)"; }
    userPin.setLocation(userloc);
    userPin.setOptions({ visible: true });
    userInfoBox.setLocation(userloc);
    userInfoBox.setOptions({ description: text, visible: true });
    userLayer.add(userPin);
}
function DrawUserLocationCircle(location) {
    var earthRadiusInMeters = 6367.0 * 1000.0;
    var lat = ToRadian(location.latitude);
    var lng = ToRadian(location.longitude);
    var d = 500 / earthRadiusInMeters;
    var locations = [];
    for (var x = 0; x <= 360; x += 3) {
        var brng = ToRadian(x);
        var latRadians = Math.asin(Math.sin(lat) * Math.cos(d) + Math.cos(lat) * Math.sin(d) * Math.cos(brng));
        var lngRadians = lng + Math.atan2(Math.sin(brng) * Math.sin(d) * Math.cos(lat), Math.cos(d) - Math.sin(lat) * Math.sin(latRadians));
        locations.push(new Microsoft.Maps.Location(ToDegrees(latRadians), ToDegrees(lngRadians)));
    }
    userLayer.add(new Microsoft.Maps.Polygon(locations, { strokeColor: "rgba(255,0,0,1)", fillColor: "rgba(125,0,255,0.2)", strokeThickness: 1 }));
}
//#region User location from click
function LocationfromMouseCoords(mouseX, mouseY, text) {
    if (text === void 0) { text = "Clicked location (loading address..)"; }
    var point = new Microsoft.Maps.Point(mouseX, mouseY);
    var loc = bingMap.map.tryPixelToLocation(point, Microsoft.Maps.PixelReference.page);
    var locString = loc.toString();
    console.log("locString:" + loc.toString());
    var locSubstring = locString.substring(14, locString.length - 2);
    console.log("locSubstring:" + locSubstring);
    var locSplit = locSubstring.split(", ");
    console.log("locSplit[0]:" + locSplit[0]);
    console.log("locSplit[1]:" + locSplit[1]);
    var lat = locSplit[0];
    var lon = locSplit[1];
    var location = new Microsoft.Maps.Location(lat, lon);
    userpinCoords = [];
    userpinCoords.push(locSplit[0]);
    userpinCoords.push(locSplit[1]);
    userLayer.clear();
    AddUserLocation(location, text);
    DrawUserLocationCircle(location);
}
function LocationfromMouseCoordsDummy(mouseX, mouseY, text) {
    if (text === void 0) { text = "Woop"; }
    var lat = "52.21574650457429";
    var lon = "6.0037776067171755";
    userpinCoords = [];
    userpinCoords.push(lat);
    userpinCoords.push(lon);
}
function UpdateLocationInfoBox(text) {
    userInfoBox.setOptions({ description: text, visible: true });
}
//#endregion
//#endregion
function AddPushpin(color, latitude, longitude) {
    console.log("Hi");
    var pushpin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(latitude, longitude), {
        icon: '<svg xmlns:svg="http://www.w3.org/2000/svg" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="25" height="40"><circle cx="12.5" cy="14.5" r="10" fill="{color}"/><image x="0" y="0" height="40" width="25" xlink:href="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABkAAAAnCAYAAAGNntMoAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAACxMAAAsTAQCanBgAAAPBSURBVEhLtZY/SGRXFMaHpE0bSJVkt8oKAf+CKytispGIjY1NcGXF/QM2wlYLgyCC+bPYpUhSCgZiJhapxEAKC0HE3cUtBLETFrExG2zMkPHm+5059/nmvTG+bHY+OL57vvOd79775s4dSzmMjY297cP6uKur67Pz8/NPFYOMrXJ0dBQIS5qis7PzgQ9LJfXesEEI4baSTzCLxIVJQzI6OmpJJGOO8wsbZJDwh4eHQRM8VzxV8w65FSK6u7vvYMvTqUb09vY2dkRoji7C0zqwSsPI4+NjGyj/KG7fuESRgnH8EeL7sXcEUero6PgclYhnyncYR44VfYwKMDYywvn8Xnp6egLhaUHI/8f19XU3rYMc3iUXwB4w1sq/9/gujuFroXaxDB2uJ7HhKqBDz5J++y9N6C1ZWFiIS/smhupfExp/pfgyrUugaadWV1c5YS+dMpDDU3cqj6GhIVaRgNxL/461tTVr4OnU1dAhuVmr1QJPp1oAbgm91i8Ud51qDr2ZWxMTE+H09NT2AqrVaoCj5rI6RJTTX1zXJzk1NJawhN3d3aSozyJ31gAGdqlx6FSoOk/DjXQ4bbADurS0VPi9m/bg4KBwg2m3trYKN5h2enraGrSn24TWzSVNJPcHgca0el2PeQMiLz3yPNGgpbE0MDBAHi/UZ4qnMUTvUENj4oi+vj743H7gqHnaCC7dLC69iIE+mPc43mnAebk5ZmdnXRoCY6cvB5eb65tfdFlwwE5OTgJhh60IJicnA+Hp1dBGbxKethD6lRzQcfiJ78H4+Hgol8uBW3Fubs5yeOrovKUY1PRocHDQ7jSgI9Vwo2ZBHR16+uj3Uh78tg8PDwd+c4HTBhlxltPBubaxSwxn539ZPz65/xU0e3l+fj5nDmQ0Kfpus4g1lyYQF/DD1wgNbk1NTcEXPxYFgB+++DPJ79vb2y2ZBF/8+Qo939/fb8kk+OLPTp5w88ZJ9Liud30tHeI+zIb4D7Ih/n0iToIv/vbd5rzzv6+EVcVD1R+kQ9z9bJ6Ke9k4+ftVVTr7HjXcF9rWrxsbG9RsRxL/4lGJodLP2aePVwj6lAd88CPPQVt7NDIyEvb29tAmn5EM/iREvYqh/A/CJWZOH/34OH05tM12CX+oVFjs1UCHnj63KA41Pl5cXHSr5qCOzlteDzJY29zcdMtGwFN36eujra3tnf7+/nB2dubWF4Cn7tL/B52UkZmZGbeugxzeJW8GMvx2ZWXFJuBJ7qU3C13fleXlZa7xilMtwVvt7e3v+rggSqV/ABNOzES/IWvuAAAAAElFTkSuQmCC"/></svg>',
        anchor: new Microsoft.Maps.Point(12, 39),
        color: color
    });
    bingMap.map.entities.push(pushpin);
}
//#region Add icon to address
function AddPintoAddress(address, title, description, color) {
    //Microsoft.Maps.loadModule(
    //    'Microsoft.Maps.Search',
    //    function () {
    //        var searchManager = new Microsoft.Maps.Search.SearchManager(bingMap.map);
    //        var requestOptions = {
    //            bounds: bingMap.map.getBounds(),
    //            where: address,
    //            callback: function (answer, userData) {
    //                let pushpin = new Microsoft.Maps.Pushpin(answer.results[0].location, {
    //                    title: title, color: color, anchor: new Microsoft.Maps.Point(25, 25)
    //                });
    //                var infobox = new Microsoft.Maps.Infobox(answer.results[0].location, { title: title, description: description, visible: false });
    //                infobox.setMap(bingMap.map);
    //                Microsoft.Maps.Events.addHandler(pushpin, 'click', () => { infobox.setOptions({ visible: true }); });
    //                bingMap.map.entities.push(pushpin);
    //            }
    //        };
    //        searchManager.geocode(requestOptions);
    //    }
    //);
    var l = new Microsoft.Maps.Location(address[0], address[1]);
    var pushpin = new Microsoft.Maps.Pushpin(l, {
        title: title, color: color, anchor: new Microsoft.Maps.Point(25, 25)
    });
    var infobox = new Microsoft.Maps.Infobox(l, { title: title, description: description, visible: false });
    infobox.setMap(bingMap.map);
    Microsoft.Maps.Events.addHandler(pushpin, 'click', function () { infobox.setOptions({ visible: true }); });
    bingMap.map.entities.push(pushpin);
}
function AddBaseLocationPintoAddress(address) {
    var l = new Microsoft.Maps.Location(address[0], address[1]);
    var pushpin = new Microsoft.Maps.Pushpin(l, {
        title: "Base location", color: "green", anchor: new Microsoft.Maps.Point(25, 25)
    });
    userLayer.add(pushpin);
    GotoAddress(address);
}
function AddColorHousetoAddress(address, title, description, houseColor, index) {
    if (index === void 0) { index = 0; }
    var l = new Microsoft.Maps.Location(address[0], address[1]);
    var imagePath = "images/house-icon.png";
    if (houseColor == "green" || houseColor == "yellow" || houseColor == "red") {
        imagePath = "images/house-icon-" + houseColor + "2.png";
    }
    var pushpin = new Microsoft.Maps.Pushpin(l, {
        title: title, icon: imagePath, color: houseColor, anchor: new Microsoft.Maps.Point(25, 25)
    });
    var infobox = new Microsoft.Maps.Infobox(l, { htmlContent: infoboxTemplate.replace('{title}', title).replace('{description}', description), visible: false });
    infobox.setMap(bingMap.map);
    Microsoft.Maps.Events.addHandler(pushpin, 'click', function () { infobox.setOptions({ visible: true }); });
    Microsoft.Maps.Events.addHandler(infobox, 'click', function () { infobox.setOptions({ visible: false }); });
    houseLayer.add(pushpin);
    //InsertHousePin(index, pushpin);
    InsertHousePinData(index, pushpin, infobox);
    infoboxArray.push(infobox);
}
//#endregion
//#region Circle calculation functions
function CalculateCircleAtAddressWithRadius(address, radius) {
    CalculateLocationCircle(new Microsoft.Maps.Location(address[0], address[1]), (radius * 1000));
}
function DrawCircleAtAddressWithRadius(address, radius) {
    DrawLocationCircle(new Microsoft.Maps.Location(address[0], address[1]), (radius * 1000));
}
//#region Handlers
function CalculateLocationCircle(location, radiusInMeters) {
    CalculateCircle(location, radiusInMeters);
}
function DrawLocationCircle(location, radiusInMeters) {
    var locations = CalculateCircle(location, radiusInMeters);
    if (locations != null) {
        generalLayer.add(new Microsoft.Maps.Polygon(locations, { strokeColor: "rgba(0,255,0,1)", fillColor: "rgba(0,255,255,0.2)", strokeThickness: 2 }));
        bingMap.map.setView({ bounds: circleCenterRect });
    }
}
function CalculateCircle(location, radiusInMeters) {
    var earthRadiusInMeters = 6371.0 * 1000.0;
    var lat = ToRadian(location.latitude);
    var lng = ToRadian(location.longitude);
    var d = radiusInMeters / earthRadiusInMeters;
    var locations = [];
    for (var x = 0; x <= 360; x += 3) {
        var brng = ToRadian(x);
        var latRadians = Math.asin(Math.sin(lat) * Math.cos(d) + Math.cos(lat) * Math.sin(d) * Math.cos(brng));
        var lngRadians = lng + Math.atan2(Math.sin(brng) * Math.sin(d) * Math.cos(lat), Math.cos(d) - Math.sin(lat) * Math.sin(latRadians));
        locations.push(new Microsoft.Maps.Location(ToDegrees(latRadians), ToDegrees(lngRadians)));
    }
    centerLocation = location;
    if (locations.length > 0) {
        circlePolygon = new Microsoft.Maps.Polygon(locations);
        circleCenterRect = Microsoft.Maps.LocationRect.fromLocations(locations);
        return locations;
    }
    else
        return null;
}
function ToRadian(degrees) { return degrees * (Math.PI / 180); }
function ToDegrees(radians) { return radians * (180 / Math.PI); }
//#endregion
//#endregion
//#region Route between two points calculation functions
function CalculateRoute(pointA, pointB) {
    CalculateRouteWithOption(pointA, pointB, "Driving");
}
//#region Handlers
function CalculateRouteWithOption(pointA, pointB, mode) {
    Microsoft.Maps.loadModule('Microsoft.Maps.Directions', function () {
        if (!directionsManager) {
            directionsManager = new Microsoft.Maps.Directions.DirectionsManager(bingMap.map);
            Microsoft.Maps.Events.addHandler(directionsManager, 'directionsUpdated', directionsUpdatedHandler);
        }
        directionsManager.clearAll();
        if (mode == "walking") {
            directionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.walking });
        }
        else if (mode == "transit") {
            directionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.transit });
        }
        else
            directionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.driving });
        var waypoint1 = new Microsoft.Maps.Directions.Waypoint({ address: 'A', location: new Microsoft.Maps.Location(pointA[0], pointA[1]) });
        var waypoint2 = new Microsoft.Maps.Directions.Waypoint({ address: 'B', location: new Microsoft.Maps.Location(pointB[0], pointB[1]) });
        directionsManager.addWaypoint(waypoint1);
        directionsManager.addWaypoint(waypoint2);
        directionsManager.setRenderOptions({ itineraryContainer: document.getElementById('printoutPanel') });
        directionsManager.calculateDirections();
    });
}
function CalculateRouteFromLocations(pointA, titleA, pointB, titleB, mode) {
    if (titleA === void 0) { titleA = 'A'; }
    if (titleB === void 0) { titleB = 'B'; }
    if (mode === void 0) { mode = 'walking'; }
    Microsoft.Maps.loadModule('Microsoft.Maps.Directions', function () {
        if (!directionsManager) {
            directionsManager = new Microsoft.Maps.Directions.DirectionsManager(bingMap.map);
            Microsoft.Maps.Events.addHandler(directionsManager, 'directionsUpdated', function () { bingMap.map.setView({ bounds: chosenHouseRect }); });
        }
        directionsManager.clearAll();
        if (mode == "walking") {
            directionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.walking });
        }
        else if (mode == "transit") {
            directionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.transit });
        }
        else
            directionsManager.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.driving });
        var waypoint1 = new Microsoft.Maps.Directions.Waypoint({ address: titleA, location: pointA });
        var waypoint2 = new Microsoft.Maps.Directions.Waypoint({ address: titleB, location: pointB });
        directionsManager.addWaypoint(waypoint1);
        directionsManager.addWaypoint(waypoint2);
        directionsManager.setRenderOptions({ itineraryContainer: document.getElementById('printoutPanel') });
        directionsManager.calculateDirections();
    });
}
function directionsUpdatedHandler() {
    console.log("Directions updated!");
    var distance = directionsManager.getRouteResult()[0].routeLegs[0].summary.distance;
    var durationSeconds = directionsManager.getRouteResult()[0].routeLegs[0].summary.time;
    var durationMinutes = durationSeconds / 60;
    var durationHours = 0;
    distance = Math.floor(distance);
    durationMinutes = Math.ceil(durationMinutes);
    var formattedString = distance + " kilometers, " + durationMinutes + " minutes.";
    if (durationMinutes >= 60) {
        durationHours = Math.floor(durationMinutes / 60);
        durationMinutes = durationMinutes - (durationHours * 60);
        formattedString = distance + " kilometers, " + durationHours + " hours and " + durationMinutes + " minutes.";
    }
    var panelID = "testPanel";
    var r = directionsManager.getRequestOptions().routeMode;
    if (r == Microsoft.Maps.Directions.RouteMode.driving) {
        panelID = panelID + "-driving";
    }
    else if (r == Microsoft.Maps.Directions.RouteMode.transit) {
        panelID = panelID + "-transit";
    }
    else if (r == Microsoft.Maps.Directions.RouteMode.walking) {
        panelID = panelID + "-walking";
    }
    if (document.getElementById(panelID)) {
        document.getElementById(panelID).innerHTML = formattedString;
    }
}
function CalculateRoutesWithOption(pointA, pointB) {
    Microsoft.Maps.loadModule('Microsoft.Maps.Directions', function () {
        if (!directionsManager1) {
            directionsManager1 = new Microsoft.Maps.Directions.DirectionsManager(bingMap.map);
            Microsoft.Maps.Events.addHandler(directionsManager1, 'directionsUpdated', function () { return directionsUpdatedHandlerWithParam(directionsManager1, "walking"); });
        }
        directionsManager1.clearAll();
        if (!directionsManager2) {
            directionsManager2 = new Microsoft.Maps.Directions.DirectionsManager(bingMap.map);
            Microsoft.Maps.Events.addHandler(directionsManager2, 'directionsUpdated', function () { return directionsUpdatedHandlerWithParam(directionsManager2, "transit"); });
        }
        directionsManager2.clearAll();
        if (!directionsManager3) {
            directionsManager3 = new Microsoft.Maps.Directions.DirectionsManager(bingMap.map);
            Microsoft.Maps.Events.addHandler(directionsManager3, 'directionsUpdated', function () { return directionsUpdatedHandlerWithParam(directionsManager3, "driving"); });
        }
        directionsManager3.clearAll();
        // Set Route Mode to driving        
        directionsManager1.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.walking });
        directionsManager2.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.transit });
        directionsManager3.setRequestOptions({ routeMode: Microsoft.Maps.Directions.RouteMode.driving });
        var waypoint1 = new Microsoft.Maps.Directions.Waypoint({ address: 'A', location: new Microsoft.Maps.Location(pointA[0], pointA[1]) });
        var waypoint2 = new Microsoft.Maps.Directions.Waypoint({ address: 'B', location: new Microsoft.Maps.Location(pointB[0], pointB[1]) });
        directionsManager1.addWaypoint(waypoint1);
        directionsManager1.addWaypoint(waypoint2);
        directionsManager1.calculateDirections();
        directionsManager2.addWaypoint(waypoint1);
        directionsManager2.addWaypoint(waypoint2);
        directionsManager2.calculateDirections();
        directionsManager3.addWaypoint(waypoint1);
        directionsManager3.addWaypoint(waypoint2);
        directionsManager3.setRenderOptions({ itineraryContainer: document.getElementById('printoutPanel') });
        directionsManager3.calculateDirections();
    });
}
function directionsUpdatedHandlerWithParam(manager, mode) {
    console.log("Directions updated!");
    var distance = manager.getRouteResult()[0].routeLegs[0].summary.distance;
    var durationSeconds = manager.getRouteResult()[0].routeLegs[0].summary.time;
    var durationMinutes = durationSeconds / 60;
    var durationHours = 0;
    distance = Math.floor(distance);
    durationMinutes = Math.ceil(durationMinutes);
    var formattedString = distance + " kilometers, " + durationMinutes + " minutes.";
    while (durationMinutes >= 60) {
        durationMinutes -= 60;
        durationHours++;
    }
    formattedString = distance + " kilometers, " + durationHours + " hours and " + durationMinutes + " minutes.";
    var panelID = "testPanel";
    if (mode == "driving") {
        panelID = panelID + "-driving";
    }
    else if (mode == "transit") {
        panelID = panelID + "-transit";
    }
    else if (mode == "walking") {
        panelID = panelID + "-walking";
    }
    if (document.getElementById(panelID)) {
        document.getElementById(panelID).innerHTML = formattedString;
    }
}
//#endregion
//#endregion
function ShowTraffic() {
    Microsoft.Maps.loadModule('Microsoft.Maps.Traffic', function () {
        var trafficManager = new Microsoft.Maps.Traffic.TrafficManager(bingMap.map);
        trafficManager.setOptions({ flowVisible: true, incidentsVisible: true, legendVisible: false });
    });
}
function SwitchLayer() {
    specificEnabled = !specificEnabled;
    if (specificEnabled) {
        //userLayer.setVisible(false);
        userInfoBox.setOptions({ visible: false });
        if (infoboxArray != null) {
            for (var _i = 0, infoboxArray_2 = infoboxArray; _i < infoboxArray_2.length; _i++) {
                var infoBox = infoboxArray_2[_i];
                infoBox.setOptions({ visible: false });
            }
        }
        if (chosenHouseRect != null) {
            bingMap.map.setView({ bounds: chosenHouseRect });
            //CalculateRouteFromLocations(chosenHouseLocation, "Chosen house", centerLocation, "Circle center", "walking");
        }
        //generalLayer.setVisible(false)
        //houseLayer.setVisible(false);
    }
    else {
        //userLayer.setVisible(true);
        if (circleCenterRect != null) {
            bingMap.map.setView({ bounds: circleCenterRect });
        }
        if (directionsManager != null) {
            directionsManager.clearDisplay();
        }
    }
    userLayer.setVisible(!specificEnabled);
    userInfoBox.setOptions({ visible: !specificEnabled });
    generalLayer.setVisible(!specificEnabled);
    isochroneLayer.setVisible(!specificEnabled);
    houseLayer.setVisible(!specificEnabled);
    specificLayer.setVisible(specificEnabled);
}
//#region Chosen house functions
function PlotRoute(mode) {
    if (mode === void 0) { mode = "walking"; }
    if (specificEnabled) {
        if (chosenHouseRect != null) {
            //CalculateRouteFromLocations(chosenHouseLocation, "Chosen house", centerLocation, "Circle center", mode);
            //CalculateRouteFromLocations(chosenHouseLocation, "", centerLocation, "", mode);
            CalculateRouteFromLocations(chosenHouseLocation, "House", centerLocation, "Base", mode);
        }
    }
}
function AddToSpecificLayer(houseAddress, houseName, baseAddress, baseName, houseColor) {
    console.log("Adding houseAddress (" + houseName + ", " + houseColor + ", " + houseAddress[0] + " " + houseAddress[1] + ") to specific layer");
    console.log("Adding baseAddress (" + baseName + ", " + baseAddress[0] + " " + baseAddress[1] + ") to specific layer");
    var h = new Microsoft.Maps.Location(houseAddress[0], houseAddress[1]);
    var imagePath = "images/house-icon.png";
    if (houseColor == "green" || houseColor == "yellow" || houseColor == "red") {
        imagePath = "images/house-icon-" + houseColor + "2.png";
    }
    var housepin = new Microsoft.Maps.Pushpin(h, {
        title: houseName, icon: imagePath, anchor: new Microsoft.Maps.Point(25, 25)
    });
    var b = new Microsoft.Maps.Location(baseAddress[0], baseAddress[1]);
    var basepin = new Microsoft.Maps.Pushpin(b, {
        title: baseName, color: 'orange'
    });
    console.log("Adding to specific layer!");
    console.log(specificLayer.getVisible());
    specificLayer.clear();
    specificLayer.add(housepin);
    specificLayer.add(basepin);
}
function SetChosenHouse(address) {
    var locations = [];
    chosenHouseLocation = new Microsoft.Maps.Location(address[0], address[1]);
    locations.push(centerLocation);
    locations.push(chosenHouseLocation);
    chosenHouseRect = Microsoft.Maps.LocationRect.fromLocations(locations);
}
//#endregion
function CheckTravelMode(travelMode) {
    var t = travelMode.toLowerCase();
    console.log("travel mode: " + t);
    if (t == "driving" || t == "walking" || t == "transit") {
        return t;
    }
    else
        return null;
}
//#region Isochrone calculation functions
function GetIsochroneByTime(address, mode, maxMinutes) {
    GetIsochrone(address, mode, maxMinutes, undefined);
}
function GetIsochroneByDistance(address, mode, maxKilometres) {
    if (mode.toLowerCase() != "transit") {
        GetIsochrone(address, mode, undefined, maxKilometres);
    }
    else
        console.log("Transit isochrone can only be calculated by travel time.");
}
//function GetIsochrone(address: string, maxMinutes: number = 10): void {
//    Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
//        var searchManager = new Microsoft.Maps.Search.SearchManager(bingMap.map);
//        var requestOptions = {
//            bounds: bingMap.map.getBounds(),
//            where: address,
//            callback: function (data, userData) {
//                var url = 'https://dev.virtualearth.net/REST/v1/Routes/Isochrones?timeUnit=minute&waypoint=' + data.results[0].location.latitude + ',' + data.results[0].location.longitude;
//                //if (document.getElementById('rbWalking').checked == true) {
//                //    url = url + '&travelMode=walking';
//                //}
//                //else if (document.getElementById('rbTransit').checked == true) {
//                //    url = url + '&travelMode=transit';
//                //}
//                //else {
//                //    url = url + '&travelMode=driving';
//                //}
//                url = url + '&travelMode=driving';
//                url = url + '&maxTime=' + maxMinutes;
//                getAjax(url, function (data) {
//                    var response = JSON.parse(data);
//                    var p = response.resourceSets[0].resources[0].polygons;
//                    var polygons = [];
//                    for (var i = 0; i < p.length; i++) {
//                        var rings = [];
//                        for (var j = 0; j < p[i].coordinates.length; j++) {
//                            var locations = [];
//                            for (var k = 0; k < p[i].coordinates[j].length; k++) {
//                                locations.push(new Microsoft.Maps.Location(p[i].coordinates[j][k][0], p[i].coordinates[j][k][1]));
//                            }
//                        }
//                        //var options = { fillColor: 'rgba(255, 255, 0, 0.3)', strokeColor: 'rgba(255, 255, 0, 0.3)' };
//                        //if (isochronePolygon != null) {
//                        //    options = { fillColor: 'rgba(0, 0, 255, 0.3)', strokeColor: 'rgba(0, 0, 255, 0.3)' };
//                        //}
//                        //polygons.push(new Microsoft.Maps.Polygon(locations, options));
//                        polygons.push(new Microsoft.Maps.Polygon(locations, { strokeColor: "rgba(0,255,0,1)", fillColor: "rgba(0,255,255,0.2)", strokeThickness: 2 }));
//                    }
//                    isochronePolygon = polygons;
//                    bingMap.map.setView({ bounds: Microsoft.Maps.LocationRect.fromShapes(polygons) });
//                    bingMap.map.entities.push(polygons);                    
//                    //bingMap.map.entities.push(new Microsoft.Maps.Pushpin(response.resourceSets[0].resources[0].origin, { color: 'orange' }));
//                });
//            }
//        };
//        searchManager.geocode(requestOptions);
//    });
//}
//#region Handler
function GetIsochrone(address, mode, maxMinutes, maxKilometres) {
    //https://dev.virtualearth.net/REST/v1/Routes/Isochrones?waypoint={waypoint}&maxtime={maxTime}&maxDistance={maxDistance}&timeUnit={timeUnit}&distanceUnit={distanceUnit}&dateTime={dateTime}&travelMode={travelMode}&key={BingMapsKey}
    //var url = 'https://dev.virtualearth.net/REST/v1/Routes/Isochrones?timeUnit=minute&waypoint=' + address[0] + ',' + address[1];
    if (maxMinutes === void 0) { maxMinutes = 10; }
    if (address.length != 2) {
        console.log("Invalid amount of coordinates: " + address.length);
        return;
    }
    var url = 'https://dev.virtualearth.net/REST/v1/Routes/Isochrones?waypoint=' + address[0] + ',' + address[1];
    var travelMode = CheckTravelMode(mode);
    if (travelMode == null) {
        console.log("Invalid travel mode: " + travelMode);
        return;
    }
    url = url + '&travelMode=' + travelMode;
    console.log("Travel mode: " + travelMode);
    if (maxKilometres && travelMode != "transit") {
        url = url + '&optimize=distance';
        url = url + '&maxDistance=' + maxKilometres;
        console.log("Calculating distance isochrone");
    }
    else {
        url = url + '&timeunit=minute';
        url = url + '&maxTime=' + maxMinutes;
        console.log("Calculating time isochrone");
    }
    //url = url + '&travelMode=driving';
    //url = url + '&maxTime=' + maxMinutes;
    console.log("url: " + url);
    //return;
    getAjax(url, function (data) {
        var response = JSON.parse(data);
        var p = response.resourceSets[0].resources[0].polygons;
        var polygons = [];
        isochroneLayer.clear();
        for (var i = 0; i < p.length; i++) {
            var rings = [];
            for (var j = 0; j < p[i].coordinates.length; j++) {
                var locations = [];
                for (var k = 0; k < p[i].coordinates[j].length; k++) {
                    locations.push(new Microsoft.Maps.Location(p[i].coordinates[j][k][0], p[i].coordinates[j][k][1]));
                }
                //Need atleast 3 locations in a ring to create a polygon.
                if (locations.length >= 3) {
                    rings.push(locations);
                }
            }
            if (rings.length > 0) {
                //polygons.push(new Microsoft.Maps.Polygon(rings));
                polygons.push(new Microsoft.Maps.Polygon(rings, { strokeColor: "rgba(0,0,255,1)", fillColor: "rgba(255,0,255,0.2)", strokeThickness: 2 }));
            }
            //polygons.push(new Microsoft.Maps.Polygon(locations, { strokeColor: "rgba(0,0,255,1)", fillColor: "rgba(255,0,255,0.2)", strokeThickness: 2 }));
        }
        isochronePolygon = polygons;
        centerLocation = new Microsoft.Maps.Location(address[0], address[1]);
        bingMap.map.setView({ bounds: Microsoft.Maps.LocationRect.fromShapes(isochronePolygon) });
        isochroneLayer.add(isochronePolygon);
    });
}
//#endregion
//#endregion
function getAjax(url, success) {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', url + '&key=' + sessionKey, true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState > 3 && xhr.status == 200)
            success(xhr.responseText);
    };
    xhr.send();
    return xhr;
}
//#region boundary checking
function CheckAddressInsideBounds(address) {
    //CheckAddressInsideIsochrone(address);
    console.log("Address is inside isochrone: " + CheckAddressInsideIsochrone(address));
    console.log("Address is inside circle: " + CheckAddressInsideCircle(address));
}
//function CheckAddressInsideIsochrone(address: number[]): boolean {
//    if (address == null || isochronePolygon == null) { return false; }
//    let loc = new Microsoft.Maps.Location(address[0], address[1]);
//    if (loc == null) { return false; }
//    Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
//        let intersects = Microsoft.Maps.SpatialMath.Geometry.intersects(loc, isochronePolygon);
//        //console.log("Address is inside polygon: " + intersects);
//        return intersects;
//    });
//}
function CheckAddressInsideIsochrone(address, index) {
    if (address == null || isochronePolygon == null) {
        return false;
    }
    var loc = new Microsoft.Maps.Location(address[0], address[1]);
    if (loc == null) {
        return false;
    }
    console.log("isochronePolygon.length: " + isochronePolygon.length);
    if (isochronePolygon.length == 0) {
        return false;
    }
    Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
        var intersects = Microsoft.Maps.SpatialMath.Geometry.intersects(loc, isochronePolygon);
        console.log("Address is inside isochrone: " + intersects);
        if (index) {
            SetHousePinVisible(index, intersects);
        }
        else
            return intersects;
    });
}
//function CheckAddressInsideCircle(address: number[]): boolean {
//    if (address == null || circlePolygon == null) { return false; }
//    let loc = new Microsoft.Maps.Location(address[0], address[1]);
//    if (loc == null) { return false; }
//    Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
//        let intersects = Microsoft.Maps.SpatialMath.Geometry.intersects(loc, circlePolygon);
//        console.log("Address is inside circle: " + intersects);
//        return intersects;
//    });
//}
function CheckAddressInsideCircle(address, index) {
    if (address == null || circlePolygon == null) {
        return false;
    }
    var loc = new Microsoft.Maps.Location(address[0], address[1]);
    if (loc == null) {
        return false;
    }
    Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
        var intersects = Microsoft.Maps.SpatialMath.Geometry.intersects(loc, circlePolygon);
        console.log("Address is inside circle: " + intersects);
        if (index) {
            SetHousePinVisible(index, intersects);
        }
        else
            return intersects;
    });
}
//#region House pin array functions
function ClearHousePins() {
    //housePins = [];
    housePinData = [];
    console.log("Cleared housepins");
}
//function InsertHousePin(index: number, pin: Microsoft.Maps.Pushpin): void {
//    if (housePins == null) {
//        console.log("Houses was null");
//        housePins = [];
//    } //housePinData
//    housePins[index] = pin;    
//}
function InsertHousePinData(index, pin, infobox) {
    if (housePinData == null) {
        console.log("housePinData was null");
        housePinData = [];
    } //housePinData
    housePinData[index] = { pushpin: pin, infobox: infobox };
}
function CheckHousePins() {
    //if (housePins == null) { return; }
    //for (var i = 0; i < housePins.length; i++) {
    //    if (!CheckHousePinExists(i)) { console.log(i + ") undefined"); }
    //    else console.log(i + ") color=" + housePins[i].getColor() + " visible=" + housePins[i].getVisible() + " text=" + housePins[i].getText());
    //}
    if (housePinData == null) {
        return;
    }
    for (var i = 0; i < housePinData.length; i++) {
        if (!CheckHousePinExists(i)) {
            console.log(i + ") undefined");
        }
        else
            console.log(i + ") color=" + housePinData[i].pushpin.getColor() + " visible=" + housePinData[i].pushpin.getVisible() + " text=" + housePinData[i].pushpin.getText() + " infobox=" + housePinData[i].infobox);
    }
}
function SetHousePinVisible(index, visible) {
    if (!CheckHousePinExists(index)) {
        return;
    }
    console.log("Setting pin " + index + " to " + visible);
    //housePins[index].setOptions({ visible: visible });
    housePinData[index].pushpin.setOptions({ visible: visible });
    if (visible == false) {
        housePinData[index].infobox.setOptions({ visible: visible });
    }
}
function SetHousePinColor(index, color) {
    if (!CheckHousePinExists(index)) {
        return;
    }
    //housePins[index].setOptions({ color: color });
    housePinData[index].pushpin.setOptions({ color: color });
}
function CheckHousePinExists(index) {
    //if (housePins == null) { return false; }
    //if (housePins[index] == undefined) { return false; }
    if (housePinData == null) {
        return false;
    }
    if (housePinData[index] == undefined || housePinData[index] == null) {
        return false;
    }
    if (housePinData[index].pushpin == undefined || housePinData[index].pushpin == null) {
        return false;
    }
    return true;
}
function ShowAllHousePins() {
    //for (var i = 0; i < housePins.length; i++) {
    //    if (!CheckHousePinExists(i)) { return; }
    //    housePins[i].setOptions({ visible: true });
    //}
    for (var i = 0; i < housePinData.length; i++) {
        if (!CheckHousePinExists(i)) {
            return;
        }
        housePinData[i].pushpin.setOptions({ visible: true });
    }
}
function ShowHouseInsideCircle(index, address) {
    console.log("Checking house " + index);
    //SetHousePinVisible(index, CheckAddressInsideCircle(address));
    CheckAddressInsideCircle(address, index);
}
function ShowHouseInsideIsochrone(index, address) {
    //SetHousePinVisible(index, CheckAddressInsideIsochrone(address));
    CheckAddressInsideIsochrone(address, index);
}
//#endregion
//#endregion
//# sourceMappingURL=maps.js.map
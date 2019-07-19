window.onload = function () { 
    /* var req = document.getElementById('request')
    req.style.visibility = 'hidden'

    var req = document.getElementById('response')
    req.style.visibility = 'hidden' */

    document.getElementById("callbackLocation").innerHTML = "<code>" + window.location.href + "auth/callback</code>"; 
};

function authorizationEndpoint() {

    console.log("endpoint")

/*     var req = document.getElementById('request')
    req.style.visibility = 'visible'

    var resp = document.getElementById('response')
    resp.style.visibility = 'visible' */

}

function getCallbackLocation() {

    console.log(window.location.href)
    return getCallbackLocation.href + "auth/callback";
}
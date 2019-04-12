
function pushFacebookButton() {
    var facebookButton = document.querySelectorAll("a.btn-facebook");

    if (facebookButton.length > 0) {
        facebookButton[0].click();
        return "1";
    }

    return "0";
}

pushFacebookButton();

function goBackIfPossible() {
    var backButtonDiv = document.querySelectorAll(".backButtonContainer");

    if (backButtonDiv.length === 0) {
        return "0";
    } else if (backButtonDiv[0].classList.contains("backButtonContainer-disabled")) {
        return "0";
    } else {
        window.history.go(-1);
        return "1";
    }
}

goBackIfPossible();

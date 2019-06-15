namespace InitScript.Common.StartupAnimation {
    export function init() {
        try {
            var pivotItems = document.querySelectorAll(".Root__main-view nav ul li");
            var entranceItems = document.querySelectorAll(".container-fluid");

            for (var i = 0; i < pivotItems.length; i++) {
                (<HTMLElement>pivotItems[i]).style.animation = 'none';
            }
            for (var i = 0; i < entranceItems.length; i++) {
                (<HTMLElement>entranceItems[i]).style.animation = 'none';
                (<HTMLElement>entranceItems[i]).style.opacity = '0';
            }

            setTimeout(function () {
                try {
                    for (var i = 0; i < pivotItems.length; i++) {
                        (<HTMLElement>pivotItems[i]).style.animation = '';
                    }
                }
                catch (ex2) {
                    console.log(ex2);
                }
            }, 400);

            setTimeout(function () {
                try {
                    for (var i = 0; i < entranceItems.length; i++) {
                        (<HTMLElement>entranceItems[i]).style.animation = '';
                        (<HTMLElement>entranceItems[i]).style.opacity = '1';
                    }
                }
                catch (ex2) {
                    console.log(ex2);
                }
            }, 400);
        }
        catch (ex) {
            console.log(ex);
        }
    }
}
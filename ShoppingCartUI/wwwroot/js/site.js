// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
async function add(laptopId, redirect = 0, isAuthenticated) {
    //alert("laptopId: " + laptopId + " redirect: " + redirect + " isAuthenticated: " + isAuthenticated);
    if (!isAuthenticated) {
        window.location.href = '/Identity/Account/Login';
    }
    try {
        var response = await fetch('/Cart/AddItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken:
                    document.getElementById("RequestVerificationToken").value
            },
            body: JSON.stringify({ laptopId: laptopId, redirect: redirect })
        });
        if (redirect == 0) {
            if (response.ok) {
                var result = await response.json();
                var cartCountEl = document.querySelector("i[data-value]");//document.getElementById('cartCount');
                cartCountEl.dataset.value = result;
                window.location.href = '#cartCount';
            } else {
                console.error("Error:", response.body);
                // Handle error
            }
        } else {
            window.location.reload();
        }
    }
    catch (ex) {
        console.error("Error:", ex);
        // Handle error
    }
}

async function remove({ laptopId, isAuthenticated } = {}) {
    if (!isAuthenticated) {
        window.location.href = '/Identity/Account/Login';
    }
    try {
        var response = await fetch('/Cart/RemoveItem', {
            method: 'Delete',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken:
                    document.getElementById("RequestVerificationToken").value
            },
            body: JSON.stringify({ laptopId: laptopId })
        });

        if (response.ok) {
            window.location.reload();
        } else {
            console.error("Error:", response.body);
            // Handle error
        }
    }
    catch (ex) {
        console.error("Error:", ex);
        // Handle error
    }
}

async function checkout({ isAuthenticated }) {
    if (!isAuthenticated) {
        window.location.href = '/Identity/Account/Login';
    }
    try {
        var response = await fetch('/Cart/GoToCheckout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken:
                    document.getElementById("RequestVerificationToken").value
            },
            body: null
        });

        if (response.ok) {
            window.location.href = "/Home/Index";
        } else {
            console.error("Error:", response.body);
        }
    }
    catch (ex) {
        console.error("Error:", ex);
    }
}
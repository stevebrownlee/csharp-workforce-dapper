// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const decomission = document.querySelector(".btn--cancelDecomission")

if (decomission) {
    decomission.addEventListener("click", e => {
        e.preventDefault()
        window.location.replace("/Computer")
    })
}

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById("LoanAmount").addEventListener("blur", function () {
    var amount = parseFloat(this.value.replace(/[^0-9.]/g, ''));
    if (amount < 1000000 || amount > 70000000) {
        document.getElementById("amountError").style.display = "block";
    } else {
        document.getElementById("amountError").style.display = "none";
    }
});

document.getElementById("InterestRate").addEventListener("blur", function () {
    var interestRate = this.value;
    if (interestRate.includes('.')) {
        document.getElementById("interestError").style.display = "block";
    } else {
        document.getElementById("interestError").style.display = "none";
    }
});

document.addEventListener('DOMContentLoaded', function () {
    var modalBtn = document.getElementById('openModalBtn');
    var modal = new bootstrap.Modal(document.getElementById('welcomeModal'));
    modalBtn.click(); // Abre automáticamente el modal
});

function validarFormulario() {
    // Obtener los valores de los campos de entrada
    var monto = document.getElementById("LoanAmount").value;
    var interes = document.getElementById("InterestRate").value;
    var cuotas = document.getElementById("NumberOfPayments").value;

    // Verificar si algún campo está vacío
    if (monto.trim() === "" || interes.trim() === "" || cuotas.trim() === "") {
        // Mostrar el mensaje de error
        document.getElementById("errorMessage").style.display = "block";

        // Ocultar el mensaje de error después de 3 segundos
        setTimeout(function () {
            document.getElementById("errorMessage").style.display = "none";
        }, 3000);

        return false; // Detener el envío del formulario
    }

    // Si todos los campos están completos, enviar el formulario
    return true;
}
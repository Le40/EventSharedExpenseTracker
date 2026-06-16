// TOASTS
window.Toast = {
    show: showToast
};

document.body.addEventListener("showToast", event => {
    Toast.show(
        event.detail.message,
        event.detail.type
    );
});


function showToast(message, type = "info") {
    const container = document.getElementById("toastContainer");

    if (!container) {
        return;
    }

    const bgClass = getToastBackgroundClass(type);

    const toastElement = document.createElement("div");
    toastElement.className = `toast align-items-center text-bg-${bgClass} border-0`;
    toastElement.setAttribute("role", "alert");
    toastElement.setAttribute("aria-live", "assertive");
    toastElement.setAttribute("aria-atomic", "true");

    toastElement.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button"
                    class="btn-close btn-close-white me-2 m-auto"
                    data-bs-dismiss="toast"
                    aria-label="Close"></button>
        </div>
    `;

    container.appendChild(toastElement);

    const toast = new bootstrap.Toast(toastElement, {
        delay: 5000,
        autohide: true
    });

    toast.show();

    toastElement.addEventListener("hidden.bs.toast", () => {
        toastElement.remove();
    });
}

function getToastBackgroundClass(type) {
    switch (type) {
        case "success":
            return "success";
        case "warning":
            return "warning";
        case "danger":
        case "error":
            return "danger";
        case "info":
        default:
            return "info";
    }
}
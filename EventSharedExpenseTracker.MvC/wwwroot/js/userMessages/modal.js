document.body.addEventListener("htmx:confirm", event => {
    const message = event.target.dataset.confirmMessage;

    if (!message) {
        return;
    }

    event.preventDefault();

    showConfirmModal(message, () => {
        event.detail.issueRequest(true);
    });
});

function showConfirmModal(message, onConfirm) {
    const modalElement = document.getElementById("confirmModal");
    const messageElement = document.getElementById("confirmModalMessage");
    const confirmButton = document.getElementById("confirmModalConfirmButton");

    if (!modalElement || !messageElement || !confirmButton) {
        return;
    }

    messageElement.textContent = message;

    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);

    const newConfirmButton = confirmButton.cloneNode(true);
    confirmButton.replaceWith(newConfirmButton);

    newConfirmButton.addEventListener("click", () => {
        modal.hide();
        onConfirm();
    });

    modal.show();
}
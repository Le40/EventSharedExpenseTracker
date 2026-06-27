let currentReceiptPreviewUrl = null;

document.addEventListener("change", async function (event) {
    if (event.target.id !== "receipt-upload") return;

    const file = event.target.files?.[0];
    if (!file) return;

    if (currentReceiptPreviewUrl) {
        URL.revokeObjectURL(currentReceiptPreviewUrl);
    }

    const tripId = getTripIdFromElement(event.target);

    currentReceiptPreviewUrl = URL.createObjectURL(file);

    if (!ServerStatus.isAvailable) {
        const url = "/Trips/" + tripId + "/Expenses/Add";

        const form = await getCachedCreateExpenseForm(tripId, url);
        if (!form) {
            Toast.show("Expense form is not available offline yet.", "warning");
            return;
        }

        const saveButton = form.querySelector("[hx-post]");
        const draft = buildExpenseDraftFromForm(form, saveButton);

        draft.source = "receipt";
        draft.receipt = {
            blob: file,
            name: file.name,
            type: file.type
        };
        draft.syncState = "pendingReceiptParse";

        await createOfflineExpense(draft);
        notifyOfflineExpensesChanged();
        Toast.show("Receipt saved offline. It will be parsed when server is available.", "success");
        //await renderPendingExpensesForCurrentTrip();
        //await updatePendingSyncUi();
        return;
    }

    console.log("Receipt preview stored:", currentReceiptPreviewUrl);
});

async function getCachedCreateExpenseForm(tripId, url) {

    const response = await fetch(url, {
        method: "GET",
        cache: "force-cache"
    });

    if (!response.ok) {
        return null;
    }

    const html = await response.text();
    const doc = new DOMParser().parseFromString(html, "text/html");

    return doc.querySelector("form");
}

function getTripIdFromElement(element) {
    const container = element.closest("[data-trip-id]");
    return container ? Number(container.dataset.tripId) : null;
}

// after htmx swaps in expenseForm, add to it the image that was just uploaded
document.body.addEventListener("htmx:afterSwap", function () {
    if (!currentReceiptPreviewUrl) return;

    const container = document.getElementById("receipt-preview-container");
    const image = document.getElementById("receipt-preview-image");

    if (!container || !image) return;

    image.src = currentReceiptPreviewUrl;
    container.classList.remove("d-none");

    console.log("Receipt preview inserted");
});


console.log("receipts.js loaded");
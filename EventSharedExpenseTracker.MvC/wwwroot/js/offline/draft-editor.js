// ---------------------------------------------------------------------------
// OPEN DRAFT FOR EDITING
// ---------------------------------------------------------------------------

// trigger is click on expense card
async function openExpenseDraftForEdit(localId, pendingCard) {
    const draft = await getOfflineExpense(localId);

    if (!draft || !draft.formHtml) {
        Toast.show("Offline draft form was not found.", "error");
        return;
    }
    // target of the stored formHTML is now offCanvas element.
    const target = document.getElementById("appOffCanvasBody");
    if (!target) {
        Toast.show("Offcanvas target was not found.", "error");
        return;
    }

    target.innerHTML = draft.formHtml;

    // if draft has image -> attach it to expense form.
    attachReceiptPreviewToDraftForm(draft);

    const form = target.querySelector("[data-offline-expense='true']");

    if (!form) {
        Toast.show("Stored form is missing offline marker.", "error");
        return;
    }

    form.dataset.offlineDraftId = draft.localId;

    const deleteButton = form.querySelector("[data-offline-draft-delete='true']");
    if (deleteButton) {
        deleteButton.classList.remove("d-none");
    }

    fillFormFromDraft(form, draft);
    renderValidationErrors(form, draft.validationErrors);
    showAppOffcanvas();
}

// ATTACH RECEIPT IMAGE TO EXPENSE FORM WRAPPER
function attachReceiptPreviewToDraftForm(draft) {
    if (!draft.receipt?.blob) return;

    const container = document.getElementById("receipt-preview-container");
    const image = document.getElementById("receipt-preview-image");

    if (!container || !image) return;

    const url = URL.createObjectURL(draft.receipt.blob);

    image.src = url;
    container.classList.remove("d-none");
}

function fillFormFromDraft(form, draft) {
    const formData = new FormData();

    for (const [key, value] of draft.fields) {
        formData.append(key, value);
    }

    const fields = form.querySelectorAll("input, select, textarea");

    fields.forEach(field => {
        if (!field.name) {
            return;
        }

        if (field.type === "checkbox") {
            const values = formData.getAll(field.name);
            field.checked = values.includes(field.value);
            return;
        }

        const value = formData.get(field.name);

        if (value !== null) {
            field.value = value;
        }
    });
}

console.log("offline 4/10 - draft-editor.js loaded");
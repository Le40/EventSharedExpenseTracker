// ---------------------------------------------------------------------------
// HANDLERS
// ---------------------------------------------------------------------------

// CREATE
async function handleExpenseDraftCreate(button) {
    const form = button.closest("form");

    const draft = await prepareExpenseDraft(button, form);
    if (!draft) return;

    draft.localId = await createOfflineExpense(draft);
    notifyOfflineExpensesChanged();
    hideAppOffcanvas();

    //form.remove();
    Toast.show("Expense saved offline. It will sync when you are online.", "success");
}

// EDIT
async function handleExpenseDraftEdit(button, form) {
    const draft = await prepareExpenseDraft(button, form);
    if (!draft) return;

    await updateOfflineExpense(draft);
    notifyOfflineExpensesChanged();
    hideAppOffcanvas();

    Toast.show("Offline draft updated.", "success");

    await syncPendingDraftsIfOnline();
}

// DELETE
async function handleExpenseDraftDelete(button) {
    const form = button.closest("form");
    const draftId = Number(form?.dataset.offlineDraftId);

    if (!draftId) return;

    showConfirmModal(
        "Delete this pending expense?",
        async () => {
            await deleteOfflineExpense(draftId);
            notifyOfflineExpensesChanged();
            hideAppOffcanvas();
        });
}

// ---------------------------------------------------------------------------
// PREPARE DRAFT
// ---------------------------------------------------------------------------

// BULID AND VALIDATE
async function prepareExpenseDraft(button, form) {
    const draft = buildExpenseDraftFromForm(form, button);
    const errors = validateExpenseDraft(draft);

    if (errors.length > 0) {
        Toast.show(errors.join("\n"), "error");
        return null;
    }

    return draft;
}

// VALIDATE
function validateExpenseDraft(draft) {
    const errors = [];
    const values = getExpenseValuesFromFields(draft.fields);

    if (!values.name?.trim()) errors.push("Expense name is required.");
    if (!values.category) errors.push("Category is required.");
    if (!values.currency) errors.push("Currency is required.");
    if (values.amount <= 0) errors.push("At least one paid amount is required.");

    return errors;
}

// BUILD
function buildExpenseDraftFromForm(form, button) {

    // Mark as offline expense so openOfflineDraftForEdit distinguish it.
    form.dataset.offlineExpense = "true";
    // wrapper arounf the form, cause it contains space for recipt photo.
    const wrapper = form.closest("[data-expense-form-wrapper='true']");

    const draft = createManualExpenseDraft({ wrapper, form, button});

    const existingDraftId = form.dataset.offlineDraftId;

    if (existingDraftId) {
        draft.localId = Number(existingDraftId);
        draft.syncState = "pendingCreate";
    }

    return draft;
}



// EDIT FORM
// trigger dbl click from card creation
async function openOfflineDraftForEdit(localId, pendingCard) {
    /*const drafts = await getAllOfflineExpenses();
    const draft = drafts.find(d => d.localId === localId);*/

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

    ////////// atach image ///////////////////////////
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
    renderOfflineValidationErrors(form, draft.validationErrors);
    showAppOffcanvas();
}

function attachReceiptPreviewToDraftForm(draft) {
    if (!draft.receipt?.blob) return;

    const container = document.getElementById("receipt-preview-container");
    const image = document.getElementById("receipt-preview-image");

    if (!container || !image) return;

    const url = URL.createObjectURL(draft.receipt.blob);

    image.src = url;
    container.classList.remove("d-none");
}

/* // mOVED TO UX.JS
function showAppOffcanvas() {
    bootstrap.Offcanvas
        .getOrCreateInstance(document.getElementById("appOffcanvas"))
        .show();
}

function hideAppOffcanvas() {
    bootstrap.Offcanvas
        .getInstance(document.getElementById("appOffcanvas"))
        ?.hide();
}*/

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



console.log("offline 5/10 - editing.js loaded");
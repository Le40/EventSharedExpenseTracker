
async function handleOfflineDraftEdit(button, form) {
    const draft = await saveOfflineDraft(button, form);
    if (!draft) return;

    //alert("Offline draft updated.");
    Toast.show("Offline draft updated.", "success");

    await syncPendingDraftsIfOnline();
}

async function handleOfflineDraftSave(button) {
    const form = button.closest("form");

    const draft = await saveOfflineDraft(button, form);
    if (!draft) return;

    form.remove();
    //alert("Expense saved offline. It will sync when you are online.");
    Toast.show("Expense saved offline. It will sync when you are online.", "success");
    await updatePendingSyncUi();
}

async function saveOfflineDraft(button, form) {
    const draft = buildExpenseDraftFromForm(form, button);
    const errors = validateOfflineExpenseDraft(draft);

    if (errors.length > 0) {
        //alert(errors.join("\n"));
        Toast.show(errors.join("\n"), "error");
        return null;
    }

    draft.localId = await saveOfflineExpense(draft);
    notifyOfflineExpensesChanged();

    await renderPendingExpensesForCurrentTrip();
    hideAppOffcanvas();

    return draft;
}

function buildExpenseDraftFromForm(form, button) {
    const formData = new FormData(form);
    const fields = Array.from(formData.entries());

    const amount = fields
        .filter(([key]) => key.endsWith(".PaidAmount"))
        .map(([, value]) => Number(value || 0))
        .reduce((sum, value) => sum + value, 0);

    const categorySelect = form.querySelector("[name='Category']");
    const rawDate = formData.get("Date");
    const currency = formData.get("CurrencyCode");

    // Mark stored form as offline draft so openOfflineDraftForEdit can find it.
    form.dataset.offlineExpense = "true";

    const draft = {
        tripId: Number(form.dataset.tripId),
        name: formData.get("Name"),
        amount,
        formattedAmount: `${amount.toFixed(2)} ${currency}`,
        currency: formData.get("CurrencyCode"),
        category: categorySelect?.selectedOptions?.[0]?.textContent?.trim() ?? "Pending",
        formattedDate: rawDate ? new Date(rawDate).toLocaleDateString() : "Pending",
        url: button.getAttribute("hx-post"),
        fields,
        syncState: "pendingCreate",
        createdAt: new Date().toISOString(),
        formHtml: form.outerHTML
    };

    const existingDraftId = form.dataset.offlineDraftId;

    if (existingDraftId) {
        draft.localId = Number(existingDraftId);
        draft.syncState = "pendingCreate";
    }

    return draft;
}

function validateOfflineExpenseDraft(draft) {
    const errors = [];

    if (!draft.name?.trim()) {
        errors.push("Expense name is required.");
    }

    if (!draft.category || draft.category === "Select Category") {
        errors.push("Category is required.");
    }

    if (!draft.currency) {
        errors.push("Currency is required.");
    }

    if (draft.amount <= 0) {
        errors.push("At least one paid amount is required.");
    }

    return errors;
}

// EDIT FORM
// trigger dbl click from card creation
async function openOfflineDraftForEdit(localId, pendingCard) {
    const drafts = await getAllOfflineExpenses();
    const draft = drafts.find(d => d.localId === localId);

    if (!draft || !draft.formHtml) {
        //alert("Offline draft form was not found.");
        Toast.show("Offline draft form was not found.", "error");
        return;
    }
    // target of the stored formHTML is now offCanvas element.
    const target = document.getElementById("appOffCanvasBody");
    if (!target) {
        Toast.show("Offcanvas target was not found.", "error");
        return;
    }

    //const pendingCard = document.querySelector(`[data-offline-draft-id='${localId}']`);

    //if (!pendingCard) {
    //    return;
    //}

    /*// storing the pending card html before swaping it for expenseForm - so cancel works
    pendingCard.dataset.originalHtml = pendingCard.innerHTML;
    pendingCard.dataset.restorable = "true";

    pendingCard.innerHTML = draft.formHtml;*/

    target.innerHTML = draft.formHtml;

    const form = target.querySelector("[data-offline-expense='true']");

    if (!form) {
        //alert("Stored form is missing offline marker.");
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

function showAppOffcanvas() {
    bootstrap.Offcanvas
        .getOrCreateInstance(document.getElementById("appOffcanvas"))
        .show();
}

function hideAppOffcanvas() {
    bootstrap.Offcanvas
        .getInstance(document.getElementById("appOffcanvas"))
        ?.hide();
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

async function handleOfflineDraftDelete(button) {
    const form = button.closest("form");
    const draftId = Number(form?.dataset.offlineDraftId);

    if (!draftId) {
        return;
    }

    //if (!confirm("Delete this pending expense?")) {
    //    return;
    //}

    showConfirmModal(
        "Delete this pending expense?",
        async () => {
            await deleteOfflineExpense(draftId);
            notifyOfflineExpensesChanged();
            hideAppOffcanvas();
            await renderPendingExpensesForCurrentTrip();
            await updatePendingSyncUi();
    });
}


/*// FORBIDDING OPENING EDIT FORM OFFLINE FOR NORMAL SYNCED EXPENSES.
document.body.addEventListener("htmx:beforeRequest", event => {
    const trigger = event.detail.elt;

    const isPendingDraft = trigger.closest("[data-pending-expense-card='true']");
    if (isPendingDraft) {
        return;
    }

    const isExpenseEdit =
        trigger.closest("[data-expense-card='true']") ||
        trigger.matches("[data-expense-card='true']");

    if (!isExpenseEdit) {
        return;
    }

    if (ServerStatus.isAvailable) {
        return;
    }

    event.preventDefault();
    Toast.show("Existing expenses can be edited when the server is available.", "info");
});*/



console.log("offline 1/5 - editing.js loaded");

async function handleOfflineDraftEdit(button, form) {
    const draft = await saveOfflineDraft(button, form);
    if (!draft) return;

    alert("Offline draft updated.");

    await syncPendingDraftsIfOnline();
}

async function handleOfflineDraftSave(button) {
    const form = button.closest("form");

    const draft = await saveOfflineDraft(button, form);
    if (!draft) return;

    form.remove();
    alert("Expense saved offline. It will sync when you are online.");
}

async function saveOfflineDraft(button, form) {
    const draft = buildExpenseDraftFromForm(form, button);
    const errors = validateOfflineExpenseDraft(draft);

    if (errors.length > 0) {
        alert(errors.join("\n"));
        return null;
    }

    draft.localId = await saveOfflineExpense(draft);
    await renderPendingExpensesForCurrentTrip();

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

async function openOfflineDraftForEdit(localId) {
    const drafts = await getAllOfflineExpenses();
    const draft = drafts.find(d => d.localId === localId);

    if (!draft || !draft.formHtml) {
        alert("Offline draft form was not found.");
        return;
    }

    const pendingCard = document.querySelector(`[data-offline-draft-id='${localId}']`);

    if (!pendingCard) {
        return;
    }
    // storing the pending card html before swaping it for expenseForm - so cancel works
    pendingCard.dataset.originalHtml = pendingCard.innerHTML;
    pendingCard.dataset.restorable = "true";

    pendingCard.innerHTML = draft.formHtml;

    const form = pendingCard.querySelector("[data-offline-expense='true']");

    if (!form) {
        alert("Stored form is missing offline marker.");
        return;
    }

    form.dataset.offlineDraftId = draft.localId;

    const deleteButton = form.querySelector("[data-offline-draft-delete='true']");
    if (deleteButton) {
        deleteButton.classList.remove("d-none");
    }

    fillFormFromDraft(form, draft);
    renderOfflineValidationErrors(form, draft.validationErrors);
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

    if (!confirm("Delete this pending expense?")) {
        return;
    }

    await deleteOfflineExpense(draftId);
    await renderPendingExpensesForCurrentTrip();
}
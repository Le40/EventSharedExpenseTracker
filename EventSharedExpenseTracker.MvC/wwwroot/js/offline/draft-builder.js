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

    // Mark as offline expense so openExpenseDraftForEdit distinguish it.
    form.dataset.offlineExpense = "true";
    // wrapper arounf the form, cause it contains space for recipt photo.
    const wrapper = form.closest("[data-expense-form-wrapper='true']");

    const draft = createManualExpenseDraft({ wrapper, form, button });

    const existingDraftId = form.dataset.offlineDraftId;

    if (existingDraftId) {
        draft.localId = Number(existingDraftId);
        draft.syncState = "pendingCreate";
    }

    return draft;
}

// CREATE BASE FOR MANUAL AND RECEIPT
function createExpenseDraftBase({ tripId, source, fields = {}, receipt = {}, syncState }) {
    return {
        offlineClientId: crypto.randomUUID(), // to prevent duplicates
        tripId,
        source,
        fields,
        receipt,
        syncState,
        createdAt: new Date().toISOString()
    };
}
// CREATE MANUAL DRAFT
function createManualExpenseDraft({ wrapper, form, button }) {
    const formData = new FormData(form);
    const fields = Array.from(formData.entries());
    return {
        ...createExpenseDraftBase({
            tripId: Number(form.dataset.tripId),
            source: "manual",
            fields,
            receipt: null,
            syncState: "pendingCreate"
        }),
        url: button.getAttribute("hx-post"),
        formHtml: wrapper ? wrapper.outerHTML : form.outerHTML,
        display: createDisplayFromFields(fields, form)
    };
}
// CREATE RECEIPT DRAFT
function createReceiptExpenseDraft(file, tripId) {
    return {
        ...createExpenseDraftBase({
            tripId: tripId,
            source: "receipt",
            fields: [],
            receipt: {
                blob: file,
                name: file.name,
                type: file.type,
            },
            syncState: "pendingReceiptParse"
        }),
        url: null,
        formHtml: null,
        display: {
            name: "Receipt waiting to parse",
            formattedAmount: "Pending",
            category: "Pending",
            formattedDate: "Pending"
        }
    };
}

// HELPERS
function createDisplayFromFields(fields, form) {
    const values = getExpenseValuesFromFields(fields);
    const categorySelect = form?.querySelector("[name='Category']");

    return {
        name: values.name || "Pending expense",
        amount: values.amount,
        formattedAmount: `${values.amount.toFixed(2)} ${values.currency || ""}`.trim(),
        currency: values.currency,
        category: categorySelect?.selectedOptions?.[0]?.textContent?.trim() ?? "Pending",
        formattedDate: values.date ? new Date(values.date).toLocaleDateString() : "Pending"
    };
}

function getExpenseValuesFromFields(fields) {
    const get = name => fields.find(([key]) => key === name)?.[1];

    const paidAmount = fields
        .filter(([key]) => key.endsWith(".PaidAmount"))
        .map(([, value]) => Number(value || 0))
        .reduce((sum, value) => sum + value, 0);

    return {
        name: get("Name"),
        category: get("Category"),
        currency: get("CurrencyCode"),
        date: get("Date"),
        amount: paidAmount
    };
}

function getCurrentTripIdForReceipt() {
    const expenseList = document.querySelector("[data-expense-list='true']");
    return Number(expenseList?.dataset.tripId);
}

console.log("offline 3/10 - draft-builder.js loaded");
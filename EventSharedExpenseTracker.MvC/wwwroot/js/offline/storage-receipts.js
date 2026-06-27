
async function saveOfflineReceiptDraft(file, tripId) {

    const draft = createReceiptExpenseDraft(file, tripId);

    /*const draft = createOfflineExpenseDraft({
        tripId: getCurrentTripId(),
        source: "receipt",
        fields: {},
        receipt: {
            blob: file,
            name: file.name,
            type: file.type,
        },
        formHtml: null,
        syncState: "pendingReceiptParse"
    })*/

    return await addToStore(EXPENSES_STORE, draft);
}

async function getAllOfflineReceipts() {

    return await getAllFromStore(RECEIPTS_STORE);
}


function createManualExpenseDraft({ wrapper, form, button }) {
    const formData = new FormData(form);
    const fields = Array.from(formData.entries());
    return {
        ...createDraftBase({
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

function createReceiptExpenseDraft(file, tripId) {
    return {
        ...createDraftBase({
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
function createDraftBase({ tripId, source, fields = {}, receipt = {}, syncState }) {
    return {
        tripId,
        source,
        fields,
        receipt,
        syncState,
        createdAt: new Date().toISOString()
    };
}

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


console.log("offline 3/10 - storage-receipts.js loaded");
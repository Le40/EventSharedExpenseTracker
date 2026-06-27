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

console.log("offline 5/10 - draft-handlers.js loaded");
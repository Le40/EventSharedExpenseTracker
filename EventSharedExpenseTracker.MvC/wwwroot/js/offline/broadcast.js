const OFFLINE_CHANNEL = "offline-expenses";
const EVENT_OFFLINE_EXPENSES_CHANGED = "offlineExpensesChanged";
const MSG_OFFLINE_EXPENSES_CHANGED = "OFFLINE_EXPENSES_CHANGED";

const offlineExpenseChannel = new BroadcastChannel(OFFLINE_CHANNEL);

// when this TAB caused the change, -> post message, and fire the event.
function notifyOfflineExpensesChanged() {
    offlineExpenseChannel.postMessage({
        type: MSG_OFFLINE_EXPENSES_CHANGED
    });

    refreshOfflineUi();
    //window.dispatchEvent(new Event(EVENT_OFFLINE_EXPENSES_CHANGED));
}

// when other TAB caused the change, listen for message and fire the event.
offlineExpenseChannel.addEventListener("message", event => {
    console.log("Broadcast received", event.data);

    if (event.data?.type !== MSG_OFFLINE_EXPENSES_CHANGED) return;

    refreshOfflineUi();
    //window.dispatchEvent(new Event(EVENT_OFFLINE_EXPENSES_CHANGED));
});

async function refreshOfflineUi() {
    await renderPendingExpensesForCurrentTrip();
    await updatePendingSyncUi();
}

console.log("offline 9/10 - broadcast.js loaded");
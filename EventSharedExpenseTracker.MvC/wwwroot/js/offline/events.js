const offlineExpenseChannel = new BroadcastChannel("offline-expenses");

function notifyOfflineExpensesChanged() {
    offlineExpenseChannel.postMessage({
        type: "OFFLINE_EXPENSES_CHANGED"
    });

    window.dispatchEvent(new Event("offlineExpensesChanged"));
}

offlineExpenseChannel.addEventListener("message", event => {
    console.log("Broadcast received", event.data);

    if (event.data?.type !== "OFFLINE_EXPENSES_CHANGED") return;

    window.dispatchEvent(new Event("offlineExpensesChanged"));
});

console.log("offline 7/7 - events.js loaded");
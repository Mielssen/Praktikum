const API_AUTH = "https://localhost:7227/api/auth";

const token = new URLSearchParams(window.location.search).get("token");

const formState = document.getElementById("formState");
const successState = document.getElementById("successState");
const invalidState = document.getElementById("invalidState");
const errorBox = document.getElementById("errorBox");
const submitBtn = document.getElementById("submitBtn");

function showError(message) {
    errorBox.innerText = message;
    errorBox.classList.remove("hidden");
}

function clearError() {
    errorBox.classList.add("hidden");
    errorBox.innerText = "";
}

function showState(state) {
    formState.classList.add("hidden");
    successState.classList.add("hidden");
    invalidState.classList.add("hidden");
    state.classList.remove("hidden");
}

function goToLogin() {
    window.location.href = "index.html";
}

async function submitReset() {
    clearError();

    const newPassword = document.getElementById("newPassword").value;
    const confirmPassword = document.getElementById("confirmPassword").value;

    if (!newPassword || newPassword.length < 6) {
        showError("Пароль має містити мінімум 6 символів");
        return;
    }

    if (newPassword !== confirmPassword) {
        showError("Паролі не співпадають");
        return;
    }

    submitBtn.disabled = true;
    submitBtn.innerText = "Змінюємо...";

    try {
        const res = await fetch(`${API_AUTH}/reset-password`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ token, newPassword })
        });

        if (!res.ok) {
            const err = await res.json().catch(() => null);

            if (err?.code === "INVALID_TOKEN") {
                showState(invalidState);
                return;
            }

            showError(err?.message || "Не вдалося змінити пароль");
            return;
        }

        showState(successState);
    } catch (e) {
        showError("Немає з'єднання з сервером. Спробуйте пізніше.");
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerText = "Змінити пароль";
    }
}

function init() {
    if (!token) {
        showState(invalidState);
        return;
    }
    showState(formState);
}

init();
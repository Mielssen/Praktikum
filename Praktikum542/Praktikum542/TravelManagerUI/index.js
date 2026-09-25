const API = "http://localhost:5265/api/auth";

function get(id) {
    return document.getElementById(id);
}

function getRoleFromToken(token) {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
}

async function safeJson(res) {
    try {
        return await res.json();
    } catch {
        return null;
    }
}

async function login() {
    clearErrors();

    const email = get("loginEmail").value;
    const password = get("loginPassword").value;

    const res = await fetch(API + "/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
    });

    const data = await safeJson(res);

    if (!res.ok) {
        showError("loginError", data);
        return;
    }

    localStorage.setItem("token", data.token);
    
    const role = getRoleFromToken(data.token);

    if (role === "manager") {
    window.location.href = "manager.html";
} else if (role === "admin") {
    window.location.href = "admin.html";
} else {
    window.location.href = "dashboard.html";
}
}

async function register() {
    clearErrors();

    const res = await fetch(API + "/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            email: get("regEmail").value,
            password: get("regPassword").value,
            name: get("regName").value,
            phone: get("regPhone").value,
            passportData: get("regPassport").value,
            dateOfBirth: get("regBirth").value
        })
    });

    const data = await safeJson(res);

    if (!res.ok) {
        showError("registerError", data);
        return;
    }

    localStorage.setItem("token", data.token);
    alert("Реєстрація успішна");
}

function showError(id, data) {
    let msg = "";

    if (data?.errors) {
        for (let key in data.errors) {
            msg += data.errors[key].join("\n") + "\n";
        }
    } 
    else if (data?.message) {
        msg = data.message;
    } 
    else {
        msg = "Невідома помилка сервера";
    }

    get(id).innerText = msg;
}

function clearErrors() {
    const errors = document.querySelectorAll(".error");
    errors.forEach(e => e.innerText = "");
}

function showRegister() {
    get("loginBox").classList.add("hidden");
    get("registerBox").classList.remove("hidden");
}

function showLogin() {
    get("registerBox").classList.add("hidden");
    get("loginBox").classList.remove("hidden");
}

function openForgotModal() {
    get("forgotEmail").value = get("loginEmail").value || "";
    get("forgotError").innerText = "";
    get("forgotFormState").classList.remove("hidden");
    get("forgotSuccessState").classList.add("hidden");
    get("forgotModal").classList.remove("hidden");
}

function closeForgotModal() {
    get("forgotModal").classList.add("hidden");
}

async function submitForgotPassword() {
    const email = get("forgotEmail").value.trim();
    const errorBox = get("forgotError");
    const btn = get("forgotSubmitBtn");

    errorBox.innerText = "";

    if (!email) {
        errorBox.innerText = "Введіть email";
        return;
    }

    btn.disabled = true;
    btn.innerText = "Надсилаємо...";

    try {
        const res = await fetch(API + "/forgot-password", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email })
        });

        const data = await safeJson(res);

        if (!res.ok) {
            errorBox.innerText = data?.message || "Не вдалося надіслати лист";
            return;
        }

        get("forgotFormState").classList.add("hidden");
        get("forgotSuccessState").classList.remove("hidden");
    } catch (e) {
        errorBox.innerText = "Немає з'єднання з сервером. Спробуйте пізніше.";
    } finally {
        btn.disabled = false;
        btn.innerText = "Надіслати";
    }
}
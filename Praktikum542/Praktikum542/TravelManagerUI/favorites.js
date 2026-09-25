const API_FAVORITES = "https://localhost:7227/api/favorites";
const token = localStorage.getItem("token");

if (!token) {
    window.location.href = "index.html";
}

async function loadFavorites() {
    const res = await fetch(API_FAVORITES, {
        headers: { "Authorization": "Bearer " + token }
    });

    if (!res.ok) {
        alert("Не вдалося завантажити обране");
        return;
    }

    const data = await res.json();
    renderFavorites(data);
}

function formatDate(dateString) {
    if (!dateString) return "";
    return new Date(dateString).toLocaleDateString("uk-UA", {
        year: "numeric", month: "2-digit", day: "2-digit"
    });
}

function renderFavorites(list) {
    const grid = document.getElementById("favoritesGrid");
    grid.innerHTML = "";

    if (list.length === 0) {
        grid.innerHTML = "<p style='color:#636e72;'>Список обраного порожній</p>";
        return;
    }

    list.forEach(f => {
        const card = document.createElement("div");
        card.className = "tour-card fav-card";

        card.innerHTML = `
            <div class="tour-card-body">
                <h4>${f.tourName}</h4>
                <p class="tour-card-desc">${f.description || ""}</p>
                <div class="tour-card-meta">
                    <span>${f.durationDays} дн.</span>
                    <span>Додано: ${formatDate(f.createdAt)}</span>
                </div>
                <div class="tour-card-price">${f.price} $ <span style="font-size:12px; color:#b2bec3;">/ особу</span></div>
                <div style="display:flex; gap:10px; margin-top:10px;">
                    <button class="btn-view" onclick="goToTour(${f.tourId})">Перейти на сайт</button>
                    <button class="btn-remove-fav" onclick="removeFavorite(${f.tourId})">Видалити</button>
                </div>
            </div>
        `;

        grid.appendChild(card);
    });
}

function goToTour(tourId) {
    window.location.href = "dashboard.html";
}

async function removeFavorite(tourId) {
    if (!confirm("Видалити тур з обраного?")) return;

    const res = await fetch(`${API_FAVORITES}/${tourId}`, {
        method: "DELETE",
        headers: { "Authorization": "Bearer " + token }
    });

    if (!res.ok) {
        const err = await res.json().catch(() => null);
        alert(err?.message || "Помилка");
        return;
    }

    loadFavorites();
}

function logout() {
    localStorage.removeItem("token");
    window.location.href = "index.html";
}

loadFavorites();
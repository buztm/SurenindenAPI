function isUserLoggedIn() {
    return !!ApiService.getToken();
}

function getCurrentUser() {
    const stored = localStorage.getItem('authUser');
    if (stored) {
        try {
            return JSON.parse(stored);
        } catch {
        }
    }

    const token = ApiService.getToken();
    if (!token) return null;

    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

function logout() {
    ApiService.removeToken();
    localStorage.removeItem('authUser');
    updateNavigation();
    window.location.href = '/';
}

function updateNavigation() {
    const isLoggedIn = isUserLoggedIn();
    const user = getCurrentUser();

    if (isLoggedIn && user) {
        const roles = user.roles || (user['role'] ? [user['role']] : []);
        const isAdmin = roles.includes('Admin') || roles.some(r => r && r.toLowerCase() === 'admin');
        const roleDisplay = roles.length > 0 ? roles.join(', ') : 'Kullanıcı';

        let navHtml = '';

        navHtml += '<li class="nav-item dropdown">';
        navHtml += `<a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
            👤 ${user.userName || 'Kullanıcı'} <span class="badge bg-info ms-2">${roleDisplay}</span>
        </a>`;
        navHtml += '<ul class="dropdown-menu dropdown-menu-end">';
        if (isAdmin) {
            navHtml += '<li><a class="dropdown-item" href="/admin/dashboard"><i class="bi bi-speedometer2"></i> Admin Paneli</a></li>';
            navHtml += '<li><hr class="dropdown-divider"></li>';
        }
        navHtml += '<li><a class="dropdown-item" href="/profile"><i class="bi bi-person"></i> Profilim</a></li>';
        navHtml += '<li><a class="dropdown-item" href="#" onclick="logout()"><i class="bi bi-door-left"></i> Çıkış Yap</a></li>';
        navHtml += '</ul>';
        navHtml += '</li>';

        $('#userNav').html(navHtml);
    } else {
        let navHtml = '';
        navHtml += '<li class="nav-item"><a class="nav-link" href="/auth/login">Giriş Yap</a></li>';
        navHtml += '<li class="nav-item"><a class="nav-link" href="/auth/register">Kayıt Ol</a></li>';
        $('#userNav').html(navHtml);
    }
}

$(document).ready(function() {
    updateNavigation();
});

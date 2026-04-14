const ApiService = {
    baseUrl: 'https://localhost:7008/api',
    timeout: 30000,

    getToken() {
        return localStorage.getItem('authToken');
    },

    setToken(token) {
        localStorage.setItem('authToken', token);
    },

    removeToken() {
        localStorage.removeItem('authToken');
    },

    getAuthHeader() {
        const token = this.getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    },

    async request(method, endpoint, data = null) {
        const headers = {
            'Content-Type': 'application/json',
            ...this.getAuthHeader()
        };

        const config = {
            type: method,
            url: `${this.baseUrl}${endpoint}`,
            headers: headers,
            timeout: this.timeout,
            dataType: 'json',
            crossDomain: true
        };

        if (data && (method === 'POST' || method === 'PUT')) {
            config.data = JSON.stringify(data);
            config.contentType = 'application/json; charset=utf-8';
        }

        return new Promise((resolve, reject) => {
            $.ajax(config)
                .done((response) => {
                    resolve(response);
                })
                .fail((xhr, status, error) => {
                    if (xhr.status === 401) {
                        this.removeToken();
                        window.location.href = '/auth/login';
                    }

                    const errorMsg = xhr.responseJSON?.message || xhr.responseText || error;
                    reject({
                        status: xhr.status,
                        response: xhr.responseJSON,
                        message: errorMsg
                    });
                });
        });
    },

    auth: {
        login(username, password) {
            return ApiService.request('POST', '/auth/login', { userName: username, password: password });
        },

        register(username, email, password) {
            return ApiService.request('POST', '/auth/register', { userName: username, email: email, password: password });
        },

        getUsers() {
            return ApiService.request('GET', '/auth/users');
        },

        updateUser(id, username, email, password) {
            return ApiService.request('PUT', `/auth/user/${id}`, {
                id: id,
                userName: username,
                email: email,
                password: password
            });
        },

        deleteUser(id) {
            return ApiService.request('DELETE', `/auth/user/${id}`);
        }
    },

    cars: {
        getAll() {
            return ApiService.request('GET', '/car');
        },

        getAvailable() {
            return ApiService.request('GET', '/car/available');
        },

        getById(id) {
            return ApiService.request('GET', `/car/${id}`);
        },

        filter(minPrice, maxPrice, fuelType, transmission, categoryId) {
            const params = new URLSearchParams();

            if (minPrice !== null && minPrice !== undefined) params.append('minPrice', minPrice);
            if (maxPrice !== null && maxPrice !== undefined) params.append('maxPrice', maxPrice);
            if (fuelType) params.append('fuelType', fuelType);
            if (transmission) params.append('transmission', transmission);
            if (categoryId) params.append('categoryId', categoryId);

            const queryString = params.toString();
            const endpoint = queryString ? `/car/filter?${queryString}` : '/car/filter';

            return ApiService.request('GET', endpoint);
        },

        uploadImage(file) {
            const formData = new FormData();
            formData.append('file', file);

            const headers = ApiService.getAuthHeader();

            return new Promise((resolve, reject) => {
                $.ajax({
                    url: `${ApiService.baseUrl}/car/upload`,
                    type: 'POST',
                    data: formData,
                    headers: headers,
                    processData: false,
                    contentType: false,
                    crossDomain: true,
                    success: (res) => resolve(res),
                    error: (xhr, status, error) => {
                        const message = xhr.responseJSON?.message || xhr.responseText || error;
                        reject({ status: xhr.status, response: xhr.responseJSON, message });
                    }
                });
            });
        },

        create(car) {
            return ApiService.request('POST', '/car', car);
        },

        update(id, car) {
            car.id = id;
            return ApiService.request('PUT', `/car/${id}`, car);
        },

        delete(id) {
            return ApiService.request('DELETE', `/car/${id}`);
        }
    },

    categories: {
        getAll() {
            return ApiService.request('GET', '/category');
        },

        create(category) {
            return ApiService.request('POST', '/category', category);
        },

        update(id, category) {
            return ApiService.request('PUT', `/category/${id}`, category);
        },

        delete(id) {
            return ApiService.request('DELETE', `/category/${id}`);
        }
    },

    rentals: {
        getAll() {
            return ApiService.request('GET', '/rental/getAll');
        },

        getMyRentals(userId) {
            return ApiService.request('GET', `/rental/my-rentals/${userId}`);
        },

        rent(rental) {
            return ApiService.request('POST', '/rental/rent', rental);
        },

        returnCar(rentalId) {
            return ApiService.request('POST', `/rental/return/${rentalId}`);
        }
    },

    dashboard: {
        getStats() {
            return ApiService.request('GET', '/dashboard/stats');
        },

        getMonthlyRevenue() {
            return ApiService.request('GET', '/dashboard/monthly-revenue');
        },

        getRecentRentals() {
            return ApiService.request('GET', '/dashboard/recent-rentals');
        }
    }
};
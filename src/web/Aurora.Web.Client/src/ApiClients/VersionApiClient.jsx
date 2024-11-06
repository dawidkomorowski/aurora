import { ApiClient } from "./ApiClient";

export class VersionApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll() {
        let uri = `${this.#issuesServiceUrl}/versions`;

        return ApiClient.fetch(uri);
    }

    static create(name) {
        const createVersionRequest = {
            name: name
        };

        const requestInit = {
            method: "POST",
            body: JSON.stringify(createVersionRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/versions`, requestInit);
    }

    static update(id, name) {
        const updateVersionRequest = {
            name: name
        };

        const requestInit = {
            method: "PUT",
            body: JSON.stringify(updateVersionRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/versions/${id}`, requestInit);
    }
}


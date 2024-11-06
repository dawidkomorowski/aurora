import { ApiClient } from "./ApiClient";

export class IssueApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll(filters) {
        let uri = `${this.#issuesServiceUrl}/issues`;

        if (filters?.status) {
            uri = `${uri}?status=${encodeURIComponent(filters.status)}`;
        }

        return ApiClient.fetch(uri);
    }

    static get(id) {
        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${id}`);
    }

    static create(title, description) {
        const createIssueRequest = {
            title: title,
            description: description
        };

        const requestInit = {
            method: "POST",
            body: JSON.stringify(createIssueRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues`, requestInit);
    }

    static update(id, title, description, status) {
        const updateIssueRequest = {
            title: title,
            description: description,
            status: status
        };

        const requestInit = {
            method: "PUT",
            body: JSON.stringify(updateIssueRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${id}`, requestInit);
    }
}


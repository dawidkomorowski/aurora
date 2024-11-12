import { ApiClient } from "./ApiClient";

export class IssueApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll(filters) {
        let uri = `${this.#issuesServiceUrl}/issues`;

        if (filters?.status) {
            uri = `${uri}?status=${encodeURIComponent(filters.status)}`;
        }

        if (filters?.versionId != null) {
            uri = `${uri}?versionId=${encodeURIComponent(filters.versionId)}`;
        }

        return ApiClient.fetch(uri);
    }

    static get(id) {
        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${id}`);
    }

    static create(title, description, versionId) {
        const createIssueRequest = {
            title: title,
            description: description,
            versionId: versionId
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

    static update(id, title, description, status, versionId) {
        const updateIssueRequest = {
            title: title,
            description: description,
            status: status,
            versionId: versionId
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


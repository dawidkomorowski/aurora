import { ApiClient } from "./ApiClient";

export class IssueApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll(filters) {
        const searchParams = new URLSearchParams();

        if (filters?.status) {
            searchParams.set("status", filters.status);
        }

        if (filters?.versionId != null) {
            searchParams.set("versionId", filters.versionId);
        }

        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues?${searchParams.toString()}`);
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
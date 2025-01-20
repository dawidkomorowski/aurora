import { ApiClient } from "./ApiClient";

export class ChecklistApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static get(id) {
        return ApiClient.fetch(`${this.#issuesServiceUrl}/checklists/${id}`)
    }

    static getAll(issueId) {
        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${issueId}/checklists`)
    }

    static createChecklist(issueId, title) {
        const createChecklistRequest = {
            title: title
        };

        const requestInit = {
            method: "POST",
            body: JSON.stringify(createChecklistRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${issueId}/checklists`, requestInit);
    }

    static updateChecklist(id, title) {
        const updateChecklistRequest = {
            title: title
        };

        const requestInit = {
            method: "PUT",
            body: JSON.stringify(updateChecklistRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/checklists/${id}`, requestInit);
    }

    static removeChecklist(id) {
        const requestInit = {
            method: "DELETE"
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/checklists/${id}`, requestInit);
    }

    static createChecklistItem(checklistId, content) {
        const createChecklistItemRequest = {
            content: content
        }

        const requestInit = {
            method: "POST",
            body: JSON.stringify(createChecklistItemRequest),
            headers: {
                "Content-Type": "application/json",
            }
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/checklists/${checklistId}/items`, requestInit);
    }

    static removeChecklistItem(id) {
        const requestInit = {
            method: "DELETE"
        };

        return ApiClient.fetch(`${this.#issuesServiceUrl}/checklists/items/${id}`, requestInit);
    }
}

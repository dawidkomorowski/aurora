import { ApiClient } from "./ApiClient";

export class ChecklistApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

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

        // TODO Invesitage SyntaxError: JSON.parse: unexpected end of data at line 1 column 1 of the JSON data.
        // Probably the reason is the service responding with 201 and no JSON data that is tried to be parse in ApiClient.
        return ApiClient.fetch(`${this.#issuesServiceUrl}/issues/${issueId}/checklists`, requestInit);
    }
}

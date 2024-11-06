import { ApiValidationError } from "./ApiValidationError";

export class VersionApiClient {
    static #issuesServiceUrl = `${__ISSUES_SERVICE_API_URL__}/api`;

    static getAll() {
        let uri = `${this.#issuesServiceUrl}/versions`;

        return fetch(uri).then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
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

        return fetch(`${this.#issuesServiceUrl}/versions`, requestInit).then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
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

        return fetch(`${this.#issuesServiceUrl}/versions/${id}`, requestInit).then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();

                if (ApiValidationError.isValidationError(errorResponse)) {
                    throw new ApiValidationError(errorResponse);
                }

                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        });
    }
}
import { ApiValidationError } from "./ApiValidationError";

export class ApiClient {
    static fetch(url, requestInit) {
        return fetch(url, requestInit).then(async (response) => {
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

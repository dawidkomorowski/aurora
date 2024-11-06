export class VersionNameValidator {
    static validate(versionName) {
        const validationErrors = [];
        versionName = versionName.trim();

        if (!versionName) {
            validationErrors.push("Version name cannot be empty.");
        }

        return validationErrors;
    }
}


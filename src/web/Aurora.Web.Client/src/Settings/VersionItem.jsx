import { useState, useEffect } from "react";
import { VersionNameValidator } from "./VersionNameValidator";
import { VersionValidationErrorPresenter } from "./VersionValidationErrorPresenter";
import { VersionApiClient } from "../ApiClients/VersionApiClient";

export function VersionItem({ id, name, onRefreshRequested }) {
    const [editMode, setEditMode] = useState(false);
    const [versionName, setVersionName] = useState(name);
    const [validationErrors, setValidationErrors] = useState([]);

    useEffect(() => {
        setValidationErrors(VersionNameValidator.validate(versionName));
    }, [versionName]);

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleSaveButtonClick() {
        VersionApiClient.update(id, versionName).then(() => {
            setEditMode(false);
            onRefreshRequested();
        }).catch(error => {
            console.error(error)
        });
    }

    function handleCancelButtonClick() {
        setVersionName(name);
        setEditMode(false);
    }

    function handleVersionNameInput(event) {
        setVersionName(event.target.value);
    }

    let content;
    if (editMode) {
        const hasValidationErrors = validationErrors.length != 0;

        content =
            <>
                <input value={versionName} onInput={handleVersionNameInput}></input>
                <div style={{ marginLeft: "5px" }}>
                    <button onClick={handleSaveButtonClick} disabled={hasValidationErrors}>Save</button>
                </div>
                <div style={{ marginLeft: "5px" }}>
                    <button onClick={handleCancelButtonClick}>Cancel</button>
                </div>
                <VersionValidationErrorPresenter validationErrors={validationErrors} />
            </>
    }
    else {
        content =
            <>
                <input value={versionName} disabled></input>
                <div style={{ marginLeft: "5px" }}>
                    <button onClick={handleEditButtonClick}>Edit</button>
                </div>
            </>
    }

    return (
        <div style={{ display: "flex", margin: "5px 5px 5px 0px" }}>
            {content}
        </div>
    );
}

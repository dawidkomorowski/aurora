import { useEffect, useState } from "react";
import { Checklist } from "./Checklist";
import { ChecklistApiClient } from "../../../ApiClients/ChecklistApiClient";
import { NewChecklist } from "./NewChecklist";
import { ApiValidationError } from "../../../ApiClients/ApiValidationError";

export function ChecklistsSection({ issueId }) {
    const [checklists, setChecklists] = useState([]);
    const [isAddingNewChecklist, setIsAddingNewChecklist] = useState(false);
    const [validationError, setValidationError] = useState(null);

    useEffect(() => {
        refreshChecklists();
    }, []);

    function refreshChecklists() {
        ChecklistApiClient.getAll(issueId).then(responseData => {
            setChecklists(responseData);
        }).catch(error => {
            console.error(error);
        });
    }

    function handleAddButtonClick() {
        setIsAddingNewChecklist(true);
    }

    function handleCreate(title) {
        ChecklistApiClient.createChecklist(issueId, title).then(() => {
            setValidationError(null);
            setIsAddingNewChecklist(false);
            refreshChecklists();
        }).catch(error => {
            if (error instanceof ApiValidationError) {
                setValidationError(error.errorMessages[0]);
            }
            else {
                console.error(error);
            }
        });
    }

    function handleCancel() {
        setValidationError(null);
        setIsAddingNewChecklist(false);
    }

    const checklistElements = checklists.map(c => {
        return (
            <div key={c.id} style={{ marginTop: "10px" }}>
                <Checklist checklist={c} />
            </div>
        );
    });

    let newChecklistElement = <></>
    if (isAddingNewChecklist) {
        newChecklistElement = (
            <div style={{ marginTop: "10px" }}>
                <NewChecklist onCreate={handleCreate} onCancel={handleCancel} />
            </div>
        )
    }

    let validationErrorElement = <></>
    if (validationError) {
        validationErrorElement = (
            <div style={{ color: "red" }}>
                {validationError}
            </div>
        );
    }

    return (
        <div style={{ display: "flex", justifyContent: "center", marginTop: "20px" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }}>
                <div style={{ display: "flex" }}>
                    <div>
                        <strong>Checklists</strong>
                    </div>
                    <div style={{ marginLeft: "auto" }}>
                        <button onClick={handleAddButtonClick}>Add</button>
                    </div>
                </div>
                <div>
                    {checklistElements}
                </div>
                {newChecklistElement}
                {validationErrorElement}
            </div>
        </div >
    );
}


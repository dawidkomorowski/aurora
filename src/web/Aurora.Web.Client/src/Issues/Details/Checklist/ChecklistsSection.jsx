import { useEffect, useState } from "react";
import { Checklist } from "./Checklist";
import { ChecklistApiClient } from "../../../ApiClients/ChecklistApiClient";
import { NewChecklist } from "./NewChecklist";

export function ChecklistsSection({ issueId }) {
    const [checklists, setChecklists] = useState([]);
    const [isAddingNewChecklist, setIsAddingNewChecklist] = useState(false);

    useEffect(() => {
        ChecklistApiClient.getAll(issueId).then(responseData => {
            setChecklists(responseData);
        }).catch(error => {
            console.error(error);
        });
    }, []);

    function handleAddButtonClick() {
        setIsAddingNewChecklist(true);
    }

    function handleCreate(title) {
        setIsAddingNewChecklist(false);
    }

    function handleCancel() {
        setIsAddingNewChecklist(false);
    }

    const checklistElements = checklists.map(c => {
        return (
            <div key={c.id} style={{ marginTop: "10px" }}>
                <Checklist checklist={c} />
            </div>
        );
    });

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
                <div style={{ marginTop: "10px" }}>
                    {isAddingNewChecklist ? <NewChecklist onCreate={handleCreate} onCancel={handleCancel} /> : <></>}
                </div>
            </div>
        </div >
    );
}


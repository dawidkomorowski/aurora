import { useEffect, useState } from "react";
import { Checklist } from "./Checklist";
import { ChecklistApiClient } from "../../../ApiClients/ChecklistApiClient";

export function ChecklistsSection({ issueId }) {
    const [checklists, setChecklists] = useState([]);

    useEffect(() => {
        ChecklistApiClient.getAll(issueId).then(responseData => {
            setChecklists(responseData);
        }).catch(error => {
            console.error(error);
        });
    }, []);

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
                <div>
                    <strong>Checklists</strong>
                </div>
                {checklistElements}
            </div>
        </div >
    );
}
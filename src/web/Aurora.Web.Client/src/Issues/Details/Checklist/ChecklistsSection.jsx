import { Checklist } from "./Checklist";

export function ChecklistsSection() {
    return (
        <div style={{ display: "flex", justifyContent: "center", marginTop: "20px" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }}>
                <div>
                    <strong>Checklists</strong>
                </div>
                <div style={{ marginTop: "10px" }}>
                    <Checklist title="Checklist 1" />
                </div>
                <div style={{ marginTop: "10px" }}>
                    <Checklist title="Checklist 2" />
                </div>
            </div>
        </div >
    );
}

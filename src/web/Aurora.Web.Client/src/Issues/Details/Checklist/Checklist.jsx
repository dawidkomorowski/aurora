import { ChecklistItem } from "./ChecklistItem";

export function Checklist({ checklistData }) {
    const items = checklistData.items.map(i => <ChecklistItem key={i.id} id={i.id} content={i.content} checked={i.checked} />);

    return (
        <div style={{ borderStyle: "solid", borderWidth: "2px", padding: "10px" }}>
            <div style={{ display: "flex" }}>
                <div>
                    <strong>{checklistData.title}</strong>
                </div>
                <div style={{ marginLeft: "auto" }}>
                    <button>Add item</button>
                    <button style={{ marginLeft: "5px" }}>Edit</button>
                    <button style={{ marginLeft: "5px" }}>Remove</button>
                </div>
            </div>
            <div>
                {items}
            </div>
        </div>
    );
}


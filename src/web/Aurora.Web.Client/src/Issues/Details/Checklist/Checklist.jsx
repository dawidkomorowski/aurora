import { ChecklistItem } from "./ChecklistItem";

export function Checklist({ title }) {
    const data = {
        ...mockData,
        title: title
    };

    const items = data.items.map(i => <ChecklistItem key={i.id} id={i.id} content={i.content} checked={i.checked} />);

    return (
        <div style={{ borderStyle: "solid", borderWidth: "2px", padding: "10px" }}>
            <div style={{ display: "flex" }}>
                <div>
                    <strong>{data.title}</strong>
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

const mockData = {
    id: 1,
    title: "Checklist 1",
    items: [
        {
            id: 1,
            content: "First item is unchecked.",
            checked: false
        },
        {
            id: 2,
            content: "Second item is checked.",
            checked: true
        },
        {
            id: 3,
            content: "Third item is also unchecked.",
            checked: false
        }
    ]
}
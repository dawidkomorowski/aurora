
export function Checklist({ title }) {
    const data = {
        ...mockData,
        title: title
    };

    const items = data.items.map(i => <ChecklistItem key={i.id} id={i.id} content={i.content} checked={i.checked} />);

    return (
        <div style={{ borderStyle: "solid", borderWidth: "2px", padding: "10px" }}>
            <div>
                <strong>{data.title}</strong>
            </div>
            <div>
                {items}
            </div>
        </div>
    );
}

function ChecklistItem({ id, content, checked }) {
    return (
        <div style={{ display: "flex" }}>
            <div>
                <input type="checkbox" checked={checked} />
            </div>
            <div>{content}</div>
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

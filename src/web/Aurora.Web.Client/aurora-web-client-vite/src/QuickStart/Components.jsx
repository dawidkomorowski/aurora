import { useState } from "react";

export function MyButton({ count, onClick }) {
    return (
        <button onClick={onClick}>Clicked {count} times</button>
    );
}

export function TwoButtons() {
    const [count, setCount] = useState(0);

    function handleClick() {
        setCount(count + 1)
    }

    return (
        <>
            <MyButton count={count} onClick={handleClick}></MyButton>
            <MyButton count={count} onClick={handleClick}></MyButton>
        </>
    );
}

export function AboutPage() {
    let content;
    if (Date.now() % 2) {
        content = <strong>Time is %2</strong>
    }
    else {
        content = <strong>Time is not %2</strong>
    }

    return (
        <>
            <div style={styles.aboutPage}>
                <h1>About</h1>
                <p>Hello there {window.navigator.userAgent}.<br />How do you do?</p>
                <p>{content}</p>
            </div>
        </>
    );
}

export function ShoppingList() {
    const products = [
        { title: "Cabbabe", isFruit: false, id: 1 },
        { title: "Garlic", isFruit: false, id: 2 },
        { title: "Apple", isFruit: true, id: 3 }
    ];

    const listItems = products.map(p => {
        return <li key={p.id} style={{ color: p.isFruit ? "magenta" : "darkgreen" }}>
            {p.title}
        </li>
    })

    return (
        <ul>
            {listItems}
        </ul>
    );
}

const styles = {
    aboutPage: {
        backgroundColor: "red"
    }
};
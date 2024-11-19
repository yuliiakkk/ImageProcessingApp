import { useState } from "react"

const Login = ({onSubmit}) => {
    const [name, setName] = useState('');

    const handleInput = (e) => {
        if(name !== e.target.value){
            setName(e.target.value);
        }
    }

    const handleLogin = () => {
        onSubmit(name);
    }

    return (
    <div style={{display: 'flex', flexDirection: 'column', gap: '30px', p: '30px'}}>
        <h1>Введіть своє ім'я</h1>
        <input maxLength={10} onChange={handleInput} />
        <button disabled={name === ''} onClick={handleLogin}>
            Увійти
        </button>
    </div>)
}

export default Login;
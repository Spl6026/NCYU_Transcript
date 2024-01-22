import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import queryString from "query-string";

const root = ReactDOM.createRoot(document.getElementById('root'));
const parsed = queryString.parse(window.location.search);

var program_no = "Transcript";

parsed.program_no = program_no;

fetch("http://localhost:44329/api/pub/check", {
    method: "POST",
    headers: {
        'content-type': 'application/json'
    },
    body: JSON.stringify(parsed)
}).then(function (res) {
    return res.json();
}).then(function (data) {
    if (data.Check) {
        root.render(
            <React.StrictMode>
                <App user_id={data.Msg} program_no={program_no}/>
            </React.StrictMode>
        );
    } else {
        alert(data.Msg)
    }
}).catch(function (Error) {
    console.log(Error)
})
//window.history.pushState({},null,window.location.href.split('?')[0]);





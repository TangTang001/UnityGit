import QtQuick 2.7
import QtQuick.Window 2.2
import QtWebEngine 1.2
import QtWebChannel 1.0
import QtQuick.Controls 1.1
import QtQuick.Layouts 1.1

Window {
    visible: true
    width: 800
    height: 600
    title: qsTr("Qt和Unity交互")

    QtObject {
        id: qmlObject
        WebChannel.id: "backend"
        signal qmlSignal(string msg);
        function receiveMsg(msg){
            console.log("received msg: "+msg);
            txtArea.text = msg;
        }
    }

    WebChannel {
        id: channel
        registeredObjects: [qmlObject]
    }

    Rectangle {
        width: 400
        height: 600
        x: 0
        y: 0
        TextField {
            id: txt
            x: 10
            y: 0
            width: 300
            height: 200
            placeholderText: "输入要发送到Unity的信息"
            text: ""
        }
        Button {
            x: 10
            y: 220
            width: 300
            height: 50
            text: "发送到unity"
            onClicked: {
                qmlObject.qmlSignal(txt.text);
            }
        }

        TextArea {
            id: txtArea
            x: 10
            y: 390
            width: 300
            height: 200
            readOnly: true
            text: "显示从Unity接收的信息"
        }
    }

    Rectangle {
        width: 400
        height: 590
        x: 400
        y: 0
        WebEngineView {
            id: webengineview
            anchors.fill: parent
            webChannel: channel
        }
    }

    Component.onCompleted: {
        webengineview.profile.clearHttpCache();
        webengineview.url = "qrc:/UnityBuild/index.html"
    }
}

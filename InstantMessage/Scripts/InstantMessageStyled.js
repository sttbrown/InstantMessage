///Auxillary Functions///

//function makeConvDivInvisible() {
//    var conDiv = document.getElementById('conversation-container');
//    var content = document.getElementById('conversation-content');
//    var message = document.getElementById('message-div');
//    content.style.display = 'none';
//    message.style.display = 'none';
//    conDiv.style.display = 'none';
//}

//function makeConvDivVisible() {
//    var conDiv = document.getElementById('conversation-container');
//    var content = document.getElementById('conversation-content');
//    var message = document.getElementById('message-div');
//    content.style.display = 'block';
//    message.style.display = 'block';
//    conDiv.style.display = 'block';
//}

function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}


/* Set the width of the side navigation to 250px and the left margin of the page content to 250px */
//function openNav() {
//    document.getElementById("mySideNav").style.width = "350px";
//    document.getElementById("main").style.marginLeft = "350px";

//}

//for development purposes leave nav open on load
//$(function () {
//    openNav();
//})

function openNewConversationNav() {
    document.getElementById("newConversationSideNav").style.width = "350px";
    document.getElementById("main").style.marginLeft = "350px";
}

/*NOT IN USE Set the width of the side navigation to 0 and the left margin of the page content to 0 */
function closeNav() {
    document.getElementById("mySideNav").style.width = "0";
    document.getElementById("main").style.marginLeft = "250px";
}

function closeNewConversationNav() {
    document.getElementById("newConversationSideNav").style.width = "0";
    document.getElementById("main").style.marginLeft = "350px";
}

/*
Global Variables (Should be encapsulated in functions?)
*/
var RECIPIENTS = new Set();
var LIVECONVERSATIONID = null;
var CONVERSATIONNAME = null;
var CONNECTED = false;
var CHAT = $.connection.chatHub;
var ONSCREENCONVERSATION = null;

var ALLCONTACTS = [];
var ALLCONVERSATIONS = [];

//////Models///////

function Conversation(con) {
    this.conversationID = con.ConversationID;
    this.name = con.Name;
    this.lastMessage = con.LastMessage;
    this.lastEdited = con.LastEdited;
    this.loaded = false;
    this.messages = [];
    this.addMessage = function (message) {
        this.messages.push(message);
    }
    this.unpackMessages = function () {
        return this.messages;
    }
}

function Message(message, user, conversationID) {
    this.content = message.Content;
    this.messageID = message.MessageID;
    this.sender = user;
    this.conversationID = conversationID;
    this.sent = message.Sent;
}

function Contact(user) {
    this.userID = user.UserID;
    this.userName = user.UserName;
    this.firstName = user.FirstName;
    this.LastName = user.LastName;
}


/*
Start Up and Reconnection 
*/
$.connection.hub.start()
    .done(
    $.connection.hub.logging = true,
    function () {
        console.log("success");
        CONNECTED = true;
        setConnectionStatus();
    }).fail(function () {
        CONNECTED = false;
        setConnectionStatus();
    });

//attempt to reconnect 5 seconds after connection drops//refactor this with method above
//to reduce code duplication. 
$.connection.hub.disconnected(function () {
    setTimeout(function () {
        $.connection.hub.start().done(
            $.connection.hub.logging = true,
            function () {
                console.log("success");
                CONNECTED = true;
              //  setConnectionStatus();
               
            }).fail(function () {
                CONNECTED = false;
                setConnectionStatus();
                console.log("connection attempt failed");
            })
    }, 5000); // Restart connection after 5 seconds.
});

/*Test functions. 
*/

$('#clear-messages').click(function () {
    $('#discussion').empty();
})




$('#logOffButton').click(function () {
    CHAT.connection.stop();
    console.log("CHAT disconnected");
})

/*
Functions Called on Connection/Reconnection
*/


function setConnectionStatus() {
    if (CONNECTED == true) {
        $("#connectionSpan").text("");
        $("#connectionSpan").text("Connection Status: Connected!");
    }
    else if (CONNECTED == false) {
        $("#connectionSpan").text("");
        $("#connectionSpan").text("Connection Status: No Connection");
    }
}

/*
   Conversation Side Bar
*/

CHAT.client.updateUser = function (user) {
    console.log(user);
    //add User variable here
    $("#username").text(user);
}

//generate a list of conversations in side bar upon click
$('#allCon').click(function () {
    getAllConversations();
})

function getAllConversations() {
    // $(".conItem").remove();
    CHAT.server.getAllConversations();
}


CHAT.client.allConversationsAdded = function () {
    displayConversations();
}


//Generate list of conversations on side bar
$(function () {
    CHAT.client.AddExistingConversation = function (c) {
        var con = new Conversation(c);
        ALLCONVERSATIONS.push(con);
    }
})

function testPrint() {
    console.log("All the Con ID's");
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        
        console.log(ALLCONVERSATIONS[i].conversationID);
    }
}

$('#print-id').click(function () {
    testPrint();
})

function displayConversations() {
    $("#conversation-header").empty();
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        var html = '<div class="conversation btn" id="' + ALLCONVERSATIONS[i].conversationID+'"><div class="media-body">' +
            '<h5 class="media-heading">' + ALLCONVERSATIONS[i].name + '</h5>' +
            '<small class="pull-left time">' + ALLCONVERSATIONS[i].lastMessage + '<br/></small>' +
            '<small class="pull-left time">' + ALLCONVERSATIONS[i].lastEdited + '<br/></small></div ></div >';
        $('#conversation-header').append(html);
    }
}


$('#conversation-header').on('click','div',function (event) {
    var conId = $(this).attr('id');
    //var conName = $(this).attr('data-address');
    var conName = "test";
    console.log("conID is " + conId);
    if (typeof conId !== "undefined")
    {
        accessConversation(conId, conName);
    }
})

//this can be used whether or not the conversation has already been loaded..
function setDisplayForConversation(conId) {
    //set conversationId for any new messages. 
    LIVECONVERSATIONID = conId;
    ONSCREENCONVERSATION = conId
    //reset notifications since user has accessed sideNav
    //$('#newMessageNotificationSpan').text("Notifications:");
    $('#discussion').empty();
    $('#con-title-bar').text("");
    if (CONVERSATIONNAME !== null) {
        $('#con-title-bar').text(CONVERSATIONNAME); //MIGHT NOT WORK
    }
    else {
        var name = getConversationName(conId);

        if (name !== null) {
            $('#conversation-heading').text(name);
        }
    }
}

function loadConversationFromServer(conId) {
    CHAT.server.openConversation(conId);
}


function accessConversation(conID, conName) {
    var loaded = checkIfLoaded(conID);
    if (loaded == true) {
        setDisplayForConversation(conID);
        displayCachedConversation(conID)
        console.log("loaded boolean is true");
    }
    else {
        setDisplayForConversation(conID);
        loadConversationFromServer(conID);
        console.log("loaded boolean is false");
    }
}


function displayCachedConversation(conversationID) {
    //makeConvDivVisible();
    setDisplayForConversation(conversationID);
    var allMessages = returnConversationMessages(conversationID);
    for (var i = 0; i < allMessages.length; i++) {
        // Add the message to the page.
        $('#discussion').append('<div class="msg"><div class="media-body" >' +
            '<small class="pull-right time"><i class="fa fa-clock-o"></i>' + htmlEncode(allMessages[i].sent)+'</small>'+
            '<h5 class="media-heading">' + htmlEncode(allMessages[i].sender)+'</h5>'+
            '<small class="col-sm-11">' + htmlEncode(allMessages[i].content)+'</small></div ></div >');
    }
}


CHAT.client.finishedLoadingConversation = function (conversationID) {
    setLoadedStatusTrue(conversationID);
    displayCachedConversation(conversationID);
}

/*
Displaying Conversations
*/
CHAT.client.returnConversationDetails = function (conID) {
    LIVECONVERSATIONID = conID;
    //maybe set ONSCREENCONVERSATION here as well?
}


function checkIfLoaded(conID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        //correct equality??
        if (ALLCONVERSATIONS[i].conversationID == conID) {
            if (ALLCONVERSATIONS[i].loaded === true) {
                return true;
            }
        }
    }
    return false;
}

$(function () {
    CHAT.client.setOnScreenConversation = function (conId) {
        console.log(" CHAT.client.setOnScreenConversation --" + conId);
        ONSCREENCONVERSATION = conId;
        console.log("On screen conversations set = " + ONSCREENCONVERSATION);

    }
})


// $(function () {
//   CHAT.client.updateConversations = function () {
//     conversationSort();
//   displayConversations();
// }
// })
//sort Conversations according to Last Edited.
function conversationSort() {
    ALLCONVERSATIONS.sort(function (a, b) {
        var conA = a.lastEdited.toUpperCase();
        var conB = b.lastEdited.toUpperCase();
        if (conA < conB) {
            return 1;
        }
        if (conA > conB) {
            return -1;
        }
        return 0;
    });
}

/*
Loading, Displaying, Sending Messages
*/


//load message function
$(function () {
    CHAT.client.loadMessage = function (user, message, conversationID) {
        addMessageToConversation(message, user, conversationID);
    }
})

function returnConversationMessages(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            return ALLCONVERSATIONS[i].unpackMessages();
        }
    }
}

function setLoadedStatusTrue(conversationID) {

    console.log("set loaded status true");
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            ALLCONVERSATIONS[i].loaded = true;
        }
    }
}

function addMessageToConversation(message, user, conversationID) {
    var cacheMessage = new Message(message, user, conversationID);
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == cacheMessage.conversationID) {
            ALLCONVERSATIONS[i].addMessage(cacheMessage);
        }
    }
}


















/*
Displaying Contacts, Starting New Conversation
*/
$('#new-conversation').click(function () {
    //initialise recipients
    RECIPIENTS = new Set();
    //$(".contact btn").remove();
    $('#contact-block').text("");
    if (ALLCONTACTS.length < 1) {
        CHAT.server.getContacts();
    }
    else {
        displayContacts();
    }
})

CHAT.client.passContact = function (user) {
    var contact = new Contact(user);
    ALLCONTACTS.push(contact);
}

CHAT.client.ShowContacts = function () {
    displayContacts();
}

function displayContacts() {
    $("#contact-panel").empty();
    for (var i = 0; i < ALLCONTACTS.length; i++) {
        var html = '<div class="contact btn" id="' + ALLCONTACTS[i].userID + '">' +
            '<div class="media-body">' +
            '<h5 class="media-heading">' + ALLCONTACTS[i].userID + '</h5>' +
            '</div></div>';
        $('#contact-panel').append(html);
    }
}

//Gets contactId from nav side bar
$('#contact-panel').on('click', 'div', function (event) {
    $('#discussion').empty();
    var contactId = $(this).attr('id');
    console.log("contact id is " +contactId);
    if (typeof contactId !== "undefined")
    {
        RECIPIENTS.add(contactId);

        if (RECIPIENTS.size < 2)
        {
            $('#con-title-bar').text(contactId + " ")
        }
        

        if (RECIPIENTS.size == 2)
        {
            $('#con-title-bar').text(" ");
            $('#myModal').modal('show');
        }

        if (RECIPIENTS.size > 2)
        {
            $('#con-title-bar').text(CONVERSATIONNAME + ' (' + displayRecipients() + ')');

        }
    }
})

$("#conNameButton").click(function () {
    $('#con-title-bar').text(" ");
    CONVERSATIONNAME = $('#conName').val();
    $('#con-title-bar').text(CONVERSATIONNAME + ' (' + displayRecipients() + ')');

})

function displayRecipients() {
    var recipients = "";
    RECIPIENTS.forEach(function (value) {
        recipients += value + ", ";   
    });
    return recipients;
}


//$('#startNewButton').click(function () {
//    var conId = null; //undefined
//    setDisplayForConversation(conId);
//   // closeNewConversationNav();
//})

$(function () {
    $('#send-message').click(function () {
        var message = $('#message').val();
        if (message.length > 0 && LIVECONVERSATIONID === null) {
          //  var recipientArray = [];
            recipientArray = Array.from(RECIPIENTS);
            CHAT.server.sendFirstMessage(message, recipientArray, CONVERSATIONNAME);
            // Clear text box and reset focus for next message.
            $('#message').val('').focus();
            RECIPIENTS = new Set();
            CONVERSATIONNAME = null;
        }
        else if (message.length > 0 && LIVECONVERSATIONID !== null) {
            CHAT.server.send(message, LIVECONVERSATIONID);
            $('#message').val('').focus();
        }
    })
})




//Add new sent message to page
CHAT.client.transferMessage = function (message, user, conID) {
    newMessageHandler(message, user, conID);
    conversationSort();
    displayConversations();
}
//IS THIS USED ANYMORE>>>>????
function newMessageHandler(message, user, conID) {
    var cacheMessage = addNewMessageToConversation(message, user, conID);
    // Add the message to the page if user has this conversation on screen
    if (ONSCREENCONVERSATION == cacheMessage.conversationID) {
        $('#discussion').append('<li class="chatItem"><strong>' + htmlEncode(cacheMessage.sender)
            + '</strong>: ' + htmlEncode(cacheMessage.content) + '</li>');
    }
    else {
        console.log("client does not have this conversation loaded");
    }
}

//Part 1, condition 1
CHAT.client.receiveNewConversation = function (con) {
    var newCon = new Conversation(con);
    CHAT.server.joinGroupRemotely(newCon.conversationID);
    ALLCONVERSATIONS.push(newCon);
    // newMessageHandler(message, user, con);
    conversationSort();
    displayConversations();

}

$(function () {
    CHAT.client.newMessageNotification = function (conId) {
        // html = '<a href="javascript:void(0)" id="' + conId + '" onclick="goToMessage()">click here to view conversation</a><br/>';
        $('#newMessageNotificationSpan').text("You have a new message");
        //do something with this.

    }
})

$(function () {
    CHAT.client.updateExistingConversation = function (con) {
        for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
            if (ALLCONVERSATIONS[i].conversationID == con.ConversationID) {
                ALLCONVERSATIONS[i].lastEdited = con.LastEdited;
                ALLCONVERSATIONS[i].lastMessage = con.LastMessage;
            }
        }
        conversationSort();
        displayConversations();
    }
})

$(function () {
    CHAT.client.messageHandler = function (message, user, conID) {
        var loaded = checkIfLoaded(conID);

        if (loaded == true) {
            console.log("message handler , loaded == true");
            var cacheMessage = addNewMessageToConversation(message, user, conID);

            if (ONSCREENCONVERSATION == cacheMessage.conversationID) {
                displayCachedConversation(cacheMessage.conversationID)
            }
            else {
                //do nothing
            }

        }
        else if (loaded == false) {
            if (ONSCREENCONVERSATION == conID) {
                loadConversationFromServer(conID);
            }
            else {
                //do nothing
            }
        }
    }
})

function getConversationName(conversationID) {
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == conversationID) {
            return ALLCONVERSATIONS[i].name;
        }
    }
}





function addNewMessageToConversation(message, user, conversationID) {
    var cacheMessage = new Message(message, user, conversationID);
    for (var i = 0; i < ALLCONVERSATIONS.length; i++) {
        if (ALLCONVERSATIONS[i].conversationID == cacheMessage.conversationID) {
            ALLCONVERSATIONS[i].addMessage(cacheMessage);
            // ALLCONVERSATIONS[i].lastMessage = cacheMessage.content;
            //  ALLCONVERSATIONS[i].lastEdited = cacheMessage.sent;
        }
    }
    return cacheMessage;
}


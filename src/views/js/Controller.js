function Banchmark() {
    this.url_AddServer = '/AddServer';
    this.url_DelServer = '/DelServer';
    this.url_ListServer = '/ListServer';
    this.url_NewMessage = '/NewMessage';
    this.url_SaveMessage = '/SaveMessage';
    this.url_DeleteMessage = '/DeleteMessage';
    this.url_DeleteCategoryMessages = '/DeleteCategoryMessages';
    this.url_ListMessages = '/ListMessages';
    this.url_Test = '/Test';
    this.url_Download = '/Download';
    this.url_Upload = '/Upload';
    this.url_ListMessageCategories = '/ListMessageCategories';
    this.url_GetRunerDetail = '/GetRunerDetail';
    this.url_GetFileID = '/GetFileID';
    this.url_Stop = '/Stop';
    this.url_Run = '/Run';
    this.url_GetLocalIPAddress = '/GetLocalIPAddress';
}
/**
* 'AddServer(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.AddServer = function (host, useHttp) {
    return api(this.url_AddServer, { host: host }, useHttp);
}
/**
* 'DelServer(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.DelServer = function (host, useHttp) {
    return api(this.url_DelServer, { host: host }, useHttp);
}
/**
* 'ListServer(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.ListServer = function (useHttp) {
    return api(this.url_ListServer, {}, useHttp);
}
/**
* 'NewMessage(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.NewMessage = function (useHttp) {
    return api(this.url_NewMessage, {}, useHttp);
}
/**
* 'SaveMessage(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.SaveMessage = function (msg, useHttp) {
    return api(this.url_SaveMessage, { msg: msg }, useHttp, true);
}
/**
* 'DeleteMessage(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.DeleteMessage = function (id, useHttp) {
    return api(this.url_DeleteMessage, { id: id }, useHttp);
}
/**
* 'DeleteCategoryMessages(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.DeleteCategoryMessages = function (category, useHttp) {
    return api(this.url_DeleteCategoryMessages, { category: category }, useHttp);
}
/**
* 'ListMessages(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.ListMessages = function (category, useHttp) {
    return api(this.url_ListMessages, { category: category }, useHttp);
}
/**
* 'Test(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.Test = function (message, host, useHttp) {
    return api(this.url_Test, { message: message, host: host }, useHttp);
}
/**
* 'Download(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.Download = function (useHttp) {
    return api(this.url_Download, {}, useHttp);
}
/**
* 'Upload(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.Upload = function (name, completed, data, useHttp) {
    return api(this.url_Upload, { name: name, completed: completed, data: data }, useHttp);
}
/**
* 'ListMessageCategories(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.ListMessageCategories = function (useHttp) {
    return api(this.url_ListMessageCategories, {}, useHttp);
}
/**
* 'GetRunerDetail(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.GetRunerDetail = function (useHttp) {
    return api(this.url_GetRunerDetail, {}, useHttp);
}
/**
* 'GetFileID(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.GetFileID = function (useHttp) {
    return api(this.url_GetFileID, {}, useHttp);
}
/**
* 'Stop(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.Stop = function (useHttp) {
    return api(this.url_Stop, {}, useHttp);
}
/**
* 'Run(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.Run = function (server, ipaddress, messages, setting, useHttp) {
    return api(this.url_Run, { server: server, ipaddress: ipaddress, messages: messages, setting: setting }, useHttp, true);
}
/**
* 'GetLocalIPAddress(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Banchmark.prototype.GetLocalIPAddress = function (useHttp) {
    return api(this.url_GetLocalIPAddress, {}, useHttp);
}

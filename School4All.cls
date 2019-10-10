/**
 * @File Name          : School4All.cls
 * @Description        : 
 * @Author             : Srini
 * @Group              : 
 * @Last Modified By   : Srini
 * @Last Modified On   : 10/10/2019, 12:55:35 PM
 * @Modification Log   : 
 * Ver       Date            Author      		    Modification
 * 1.0    10/10/2019   Srini     Initial Version
**/
public with sharing class School4All {
@AuraEnabled
public static List<School__c>getSchools(){
    return[Select Id, Name,Books__r.Name,StudentName__r.Name from School__c];
}    
}

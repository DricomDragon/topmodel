﻿////
//// ATTENTION, CE FICHIER EST PARTIELLEMENT GENERE AUTOMATIQUEMENT !
////

using Microsoft.AspNetCore.Mvc;

namespace CSharp.Api.Controllers.Securite.Utilisateur;

public class UtilisateurApiController : Controller
{

    /// <summary>
    /// Recherche des utilisateurs
    /// </summary>
    /// <param name="utiId">Id technique</param>
    /// <returns>Task.</returns>
    [HttpDelete("utilisateur/deleteAll")]
    public async Task deleteAll(int[] utiId = null)
    {

    }

    /// <summary>
    /// Charge le détail d'un utilisateur
    /// </summary>
    /// <param name="utiId">Id technique</param>
    /// <returns>Le détail de l'utilisateur</returns>
    [HttpGet("utilisateur/{utiId:int}")]
    public async Task<UtilisateurDto> find(int utiId)
    {

    }

    /// <summary>
    /// Charge une liste d'utilisateurs par leur type
    /// </summary>
    /// <param name="typeUtilisateurCode">Type d'utilisateur en Many to one</param>
    /// <returns>Liste des utilisateurs</returns>
    [HttpGet("utilisateur/list")]
    public async Task<IEnumerable<UtilisateurDto>> findAllByType(TypeUtilisateur.Codes typeUtilisateurCode = TypeUtilisateur.Codes.ADM)
    {

    }

    /// <summary>
    /// Sauvegarde un utilisateur
    /// </summary>
    /// <param name="utilisateur">Utilisateur à sauvegarder</param>
    /// <returns>Utilisateur sauvegardé</returns>
    [HttpPost("utilisateur/save")]
    public async Task<UtilisateurDto> save([FromBody] UtilisateurDto utilisateur)
    {

    }

    /// <summary>
    /// Recherche des utilisateurs
    /// </summary>
    /// <param name="utiId">Id technique</param>
    /// <param name="age">Age en années de l'utilisateur</param>
    /// <param name="profilId">Profil de l'utilisateur</param>
    /// <param name="email">Email de l'utilisateur</param>
    /// <param name="nom">Nom de l'utilisateur</param>
    /// <param name="typeUtilisateurCode">Type d'utilisateur en Many to one</param>
    /// <param name="dateCreation">Date de création de l'utilisateur</param>
    /// <param name="dateModification">Date de modification de l'utilisateur</param>
    /// <returns>Utilisateurs matchant les critères</returns>
    [HttpPost("utilisateur/search")]
    public async Task<ICollection<UtilisateurDto>> search(int? utiId = null, decimal age = 6l, int? profilId = null, string email = null, string nom = "Jabx", TypeUtilisateur.Codes typeUtilisateurCode = TypeUtilisateur.Codes.ADM, DateOnly? dateCreation = null, DateOnly? dateModification = null)
    {

    }

}
<?php
////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

namespace App\Model\Securite\Utilisateur;

use NotNull;
use Symfony\Component\Validator\Constraints\Length;

class UtilisateurRead
{
  #[Symfony\Component\Validator\Constraints\NotNull]
  private int $id;

  #[Symfony\Component\Validator\Constraints\NotNull]
  #[Length(max: 100)]
  private string $nom;

  #[Symfony\Component\Validator\Constraints\NotNull]
  #[Length(max: 100)]
  private string $prenom;

  #[Symfony\Component\Validator\Constraints\NotNull]
  #[Length(max: 50)]
  private string $email;

  private Date|null $dateNaissance;

  #[Length(max: 100)]
  private string|null $adresse;

  #[Symfony\Component\Validator\Constraints\NotNull]
  private bool $actif = true;

  #[Symfony\Component\Validator\Constraints\NotNull]
  private int $profilId;

  #[Symfony\Component\Validator\Constraints\NotNull]
  #[Length(max: 10)]
  private string $typeUtilisateurCode = TypeUtilisateur.Gestionnaire;

  #[Symfony\Component\Validator\Constraints\NotNull]
  private Date $dateCreation;

  private Date|null $dateModification;

  public function getId(): int
  {
    return $this->id;
  }

  public function getNom(): string
  {
    return $this->nom;
  }

  public function getPrenom(): string
  {
    return $this->prenom;
  }

  public function getEmail(): string
  {
    return $this->email;
  }

  public function getDateNaissance(): Date|null
  {
    return $this->dateNaissance;
  }

  public function getAdresse(): string|null
  {
    return $this->adresse;
  }

  public function getActif(): bool
  {
    return $this->actif;
  }

  public function getProfilId(): int
  {
    return $this->profilId;
  }

  public function getTypeUtilisateurCode(): string
  {
    return $this->typeUtilisateurCode;
  }

  public function getDateCreation(): Date
  {
    return $this->dateCreation;
  }

  public function getDateModification(): Date|null
  {
    return $this->dateModification;
  }

  public function setId(int|null $id): self
  {
    $this->id = $id;

    return $this;
  }

  public function setNom(string|null $nom): self
  {
    $this->nom = $nom;

    return $this;
  }

  public function setPrenom(string|null $prenom): self
  {
    $this->prenom = $prenom;

    return $this;
  }

  public function setEmail(string|null $email): self
  {
    $this->email = $email;

    return $this;
  }

  public function setDateNaissance(Date|null $dateNaissance): self
  {
    $this->dateNaissance = $dateNaissance;

    return $this;
  }

  public function setAdresse(string|null $adresse): self
  {
    $this->adresse = $adresse;

    return $this;
  }

  public function setActif(bool|null $actif): self
  {
    $this->actif = $actif;

    return $this;
  }

  public function setProfilId(int|null $profilId): self
  {
    $this->profilId = $profilId;

    return $this;
  }

  public function setTypeUtilisateurCode(string|null $typeUtilisateurCode): self
  {
    $this->typeUtilisateurCode = $typeUtilisateurCode;

    return $this;
  }

  public function setDateCreation(Date|null $dateCreation): self
  {
    $this->dateCreation = $dateCreation;

    return $this;
  }

  public function setDateModification(Date|null $dateModification): self
  {
    $this->dateModification = $dateModification;

    return $this;
  }
}
